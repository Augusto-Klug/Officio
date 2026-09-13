# Autenticacao JWT local

O fluxo utiliza usuarios existentes, PasswordHasher<Usuario> e Authorization: Bearer.
O login aplica Trim() ao email e consulta sem filtro de tenant, pois ainda nao existe
usuario autenticado. O indice unico de email e global. Usuario ausente, inativo ou
senha incorreta recebem a mesma resposta 401 do controller.

## Configuracao e execucao (PowerShell, raiz do repositorio)

Use um SQL Server com o schema atual. TenantId identifica logicamente Parisotto
Contabilidade; nenhuma empresa cliente precisa existir. O bootstrap nao cria empresas, nao aplica
migrations e nao altera usuarios existentes, inclusive senha, role ou estado ativo.
As migrations locais devem ser avaliadas com a equipe antes de atualizar o banco.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = '<connection string do SQL Server local>'
$env:Jwt__Key = '<chave aleatoria privada com pelo menos 32 bytes para HS256>'
$env:Jwt__Issuer = 'Officio'
$env:Jwt__Audience = 'Officio'
$env:Jwt__ExpirationHours = '8'
$env:BootstrapUser__Enabled = 'true'
$env:BootstrapUser__TenantId = '59ab6258-849c-4b30-a154-7fe583932123'
$env:BootstrapUser__Nome = 'Administrador local'
$env:BootstrapUser__Email = 'admin@officio.test'
$env:BootstrapUser__Password = '<senha privada para desenvolvimento>'
$env:BootstrapUser__Role = 'Admin'

dotnet restore src/Officio.slnx
dotnet run --project src/Officio.WebApi --no-launch-profile --urls https://localhost:7043
```

Substitua os placeholders somente no ambiente local. Nao versione segredos.
Se necessario, configure o certificado HTTPS local com `dotnet dev-certs https --trust`.
As variaveis de ambiente sao lidas nativamente; um arquivo .env nao e carregado automaticamente.

Como alternativa, o projeto possui UserSecretsId. Configure as mesmas chaves com
dois-pontos, por exemplo:

```powershell
dotnet user-secrets set 'Jwt:Key' '<chave privada>' --project src/Officio.WebApi
dotnet user-secrets set 'BootstrapUser:Password' '<senha privada>' --project src/Officio.WebApi
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' '<connection string>' --project src/Officio.WebApi
```

User-secrets sao carregados em Development. Variaveis de ambiente prevalecem.
Jwt:Key e obrigatoria; sua ausencia impede a inicializacao com mensagem explicita.
Issuer e Audience usam Officio por padrao; ExpirationHours usa 8 quando ausente.
O Bearer valida assinatura, chave, issuer, audience e validade, com ClockSkew de 30 segundos.

O bootstrap somente executa em Development com BootstrapUser:Enabled=true. Os logs
indicam configuracao incompleta ou email de outro tenant/escritorio.
Apos criar o usuario, pode-se desabilitar BootstrapUser:Enabled e remover a senha da configuracao.
Para remover o mecanismo futuramente, exclua Desenvolvimento/BootstrapUsuario.cs e sua chamada no Program.cs.

## Tenant logico do TCC

Use o mesmo GUID fixo para Parisotto Contabilidade em todas as inicializacoes e
usuarios deste ambiente. O GUID de exemplo acima nao e um segredo nem um EmpresaId.
Alternativamente: `dotnet user-secrets set 'BootstrapUser:TenantId' '59ab6258-849c-4b30-a154-7fe583932123' --project src/Officio.WebApi`.
Trocar essa configuracao nao migra usuarios ou dados existentes; o bootstrap nao os altera.

ContextoAplicacao.Empresas representa empresas clientes. O mapeamento explicito
ToTable("Tenants") preserva somente o nome fisico legado, sem criar tabela Tenant
ou migration. UsuarioEmpresa (UsuarioId, EmpresaId) representa o vinculo de acesso;
as regras completas de autorizacao por perfil ficam para outra tarefa.

IEmpresaAtual (14 referencias em 13 arquivos C#) e IntermediarioEmpresa (7 em 4)
mantem seus nomes: a renomeacao seria semantica, sem beneficio funcional imediato.
Em uma futura versao para varios escritorios, sera necessario evoluir a segregacao
das empresas clientes, que hoje nao possuem TenantId nem filtro por escritorio.

Pendencia preexistente fora deste escopo: ContratoServico.GerarPdfAsync consulta
Empresa usando empresaAtual.TenantId. A origem dos dados do escritorio no PDF deve
ser corrigida em tarefa propria; com GUID logico sem Empresa correspondente, esse
fluxo pode retornar empresa nao encontrada. O fluxo de contratos/assinaturas foi preservado.

## Exemplo de requisicoes

Em outro terminal, utilize o email e a senha configurados:

```powershell
$baseUrl = 'https://localhost:7043'
$body = @{ email = 'admin@officio.test'; senha = '<senha configurada>' } | ConvertTo-Json
$login = Invoke-RestMethod -Method Post -Uri "$baseUrl/api/v1/autenticacao/Login" -ContentType 'application/json' -Body $body
$login | ConvertTo-Json -Depth 3
Invoke-RestMethod -Uri "$baseUrl/api/v1/clientes/Listar" -Headers @{ Authorization = "Bearer $($login.token)" }
```

A resposta do login contem token, expiraEm e usuario (id, nome, email, perfil).
O perfil corresponde a Usuario.Role. Sem o header Authorization, a mesma rota
protegida retorna 401. Com token valido, o controller consulta clientes do tenant;
o sucesso da consulta tambem depende do banco configurado e do schema compativel.

## Testes

```powershell
dotnet test src/Officio.Testes/Officio.Testes.csproj --no-restore --filter "FullyQualifiedName~ServicoJwtTestes|FullyQualifiedName~BootstrapUsuarioTestes|FullyQualifiedName~IntermediarioEmpresaTestes|FullyQualifiedName~BearerTokenTestes"
```

Cobertura: credenciais validas e invalidas,
usuario ausente/inativo, claims, normalizacao, login sem tenant previo, expiracao
configurada/padrao, chave ausente, restricoes e idempotencia do bootstrap, rotas
publicas e pipeline de autenticacao/tenant/autorizacao com metadata Authorize.
Os testes usam EF InMemory e pipeline em memoria; nao substituem uma execucao
com SQL Server e HTTP real. Nenhuma migration foi executada ou modificada nesta tarefa.
