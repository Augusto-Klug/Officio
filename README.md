# Officio

O Officio e uma aplicacao para apoiar o ciclo comercial e operacional de empresas de servicos. O repositorio contem:

- uma API ASP.NET Core em .NET 10, dentro de `src/`;
- um frontend Vue 3 + TypeScript + Vite, dentro de `frontend/`;
- persistencia em SQL Server;
- armazenamento de arquivos em Azure Blob Storage ou, localmente, no Azurite;
- integracoes opcionais com Azure Document Intelligence, Autentique e SMTP.

Este guia mostra como preparar uma maquina nova e executar o sistema completo em desenvolvimento. O caminho principal usa Docker somente para o SQL Server e o Azurite; backend e frontend rodam diretamente na maquina para facilitar o desenvolvimento.

Para preparar um piloto hospedado na Azure, consulte [docs/piloto-azure.md](docs/piloto-azure.md). O guia separa a preparacao local da ativacao com o parceiro e registra os bloqueios para uso real.

## 1. O que instalar

Instale os itens abaixo:

| Ferramenta | Versao minima | Uso |
| --- | --- | --- |
| [Git](https://git-scm.com/downloads) | atual | obter e versionar o projeto |
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10 | compilar e executar a API |
| [Node.js](https://nodejs.org/en/download) | 22.12 | executar o frontend; o npm acompanha o Node.js |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) ou Docker Engine | atual | executar SQL Server e Azurite |

O SQL Server e o Azurite nao precisam ser instalados separadamente quando se usa Docker. Em Windows, abra o Docker Desktop antes de continuar.

Confirme a instalacao em um terminal:

```bash
git --version
dotnet --version
node --version
npm --version
docker --version
```

O comando `dotnet --version` deve retornar `10.x` e `node --version`, `v22.12.0` ou superior.

## 2. Obter o projeto

Clone o repositorio e entre na pasta raiz:

```bash
git clone <URL_DO_REPOSITORIO>
cd Officio
```

Todos os comandos das proximas secoes devem ser executados a partir dessa pasta, salvo quando o texto indicar outra pasta.

## 3. Subir o SQL Server e o Azurite

### SQL Server

O comando abaixo cria um SQL Server 2022, publica a porta `1433` e mantem os dados no volume `officio-sql-data`:

```bash
docker run --name officio-sql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=OfficioDev_2026_Local" -p 1433:1433 -v officio-sql-data:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest
```

Essa senha e apenas um valor local de desenvolvimento. Se a alterar, use exatamente a mesma senha na connection string da secao seguinte. Aguarde o SQL Server ficar pronto; isso pode levar alguns segundos na primeira execucao:

```bash
docker logs -f officio-sql
```

Quando aparecer a mensagem de que o servidor esta pronto para conexoes, use `Ctrl+C` para sair dos logs. Isso nao para o container.

### Azurite

O Azurite emula o Azure Storage e permite que upload e leitura de arquivos funcionem sem uma conta Azure. O comando publica os servicos de Blob, Queue e Table nas portas padrao e persiste os arquivos no volume `officio-azurite-data`:

```bash
docker run --name officio-azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 -v officio-azurite-data:/data -d mcr.microsoft.com/azure-storage/azurite
```

Confira se os dois containers estao ativos:

```bash
docker ps
```

Esses comandos de criacao sao executados apenas uma vez. Nas proximas vezes, basta iniciar os containers existentes:

```bash
docker start officio-sql officio-azurite
```

## 4. Configurar o backend

O projeto suporta `appsettings`, o arquivo local `src/.env`, variaveis de ambiente e .NET User Secrets. Para uma instalacao nova, User Secrets e a opcao recomendada: os valores ficam fora do repositorio e nao ha risco de versionar senhas por engano.

### Banco, armazenamento e JWT

Execute:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=Officio;User Id=sa;Password=OfficioDev_2026_Local;Encrypt=False;TrustServerCertificate=True" --project src/Officio.WebApi
dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "UseDevelopmentStorage=true" --project src/Officio.WebApi
dotnet user-secrets set "Jwt:Key" "officio-local-only-change-this-key-2026-123456789" --project src/Officio.WebApi
```

A chave JWT acima e exclusiva para desenvolvimento local. Nunca a reutilize em outro ambiente.

### Primeiro usuario

O bootstrap cria o administrador somente quando ele ainda nao existe. Configure-o assim:

```bash
dotnet user-secrets set "BootstrapUser:Enabled" "true" --project src/Officio.WebApi
dotnet user-secrets set "BootstrapUser:TenantId" "59ab6258-849c-4b30-a154-7fe583932123" --project src/Officio.WebApi
dotnet user-secrets set "BootstrapUser:Nome" "Administrador local" --project src/Officio.WebApi
dotnet user-secrets set "BootstrapUser:Email" "admin@officio.test" --project src/Officio.WebApi
dotnet user-secrets set "BootstrapUser:Password" "OfficioAdmin_2026_Local" --project src/Officio.WebApi
dotnet user-secrets set "BootstrapUser:Role" "Admin" --project src/Officio.WebApi
```

As credenciais iniciais do frontend serao:

- e-mail: `admin@officio.test`;
- senha: `OfficioAdmin_2026_Local`.

Troque a senha se este ambiente for compartilhado. O bootstrap nao redefine a senha de um usuario que ja existe.

### Restaurar e executar a API

Restaure as dependencias:

```bash
dotnet restore src/Officio.slnx
```

Execute a API pelo perfil HTTP:

```bash
dotnet run --project src/Officio.WebApi --launch-profile http
```

Na primeira inicializacao em `Development`, a aplicacao:

1. conecta ao SQL Server;
2. cria o banco `Officio` e o schema completo, se o banco ainda nao existir;
3. cria o administrador configurado;
4. passa a atender em `http://localhost:5131`.

Com a API em execucao, acesse:

- Swagger UI: <http://localhost:5131/swagger>
- documento OpenAPI: <http://localhost:5131/openapi/v1.json>

### Banco de dados

As migrations versionadas sao incrementais e pressupõem que o schema base já exista. Para preparar uma instalação limpa, configure `ConnectionStrings__DefaultConnection` para apontar a um banco vazio (a variável está exemplificada, comentada, em `src/.envexample`) e execute na raiz do repositório:

```powershell
dotnet run --project src/Officio.WebApi -- --bootstrap-schema
```

O comando cria as tabelas a partir do modelo EF Core atual e registra as migrations existentes como baseline. Ele recusa bancos que já contenham tabelas de usuário.

Para atualizar uma base existente, faça backup e execute:

```powershell
dotnet run --project src/Officio.WebApi -- --apply-migrations
```

O início normal da API em produção não aplica migrations automaticamente. Em desenvolvimento, `Database:EnsureCreatedOnStartup=true` mantém o comportamento local de criar o schema e o baseline na primeira execução.

O procedimento de backup, aplicação de migrations e recuperação está em [docs/operacao-banco.md](docs/operacao-banco.md).

## 5. Configurar e executar o frontend

Abra outro terminal na raiz do repositorio.

No PowerShell:

```powershell
Copy-Item frontend/.env.example frontend/.env
Set-Location frontend
npm.cmd ci
npm.cmd run dev
```

No Bash, zsh ou terminal equivalente:

```bash
cp frontend/.env.example frontend/.env
cd frontend
npm ci
npm run dev
```

O arquivo de exemplo ja aponta o proxy do Vite para `http://localhost:5131`. Abra <http://localhost:5173> e entre com o usuario criado na secao anterior.

No Windows, os comandos usam `npm.cmd` para evitar o bloqueio de `npm.ps1` pela politica de execucao do PowerShell. Em Prompt de Comando, Bash, Linux e macOS, use `npm` normalmente.

## 6. Rotina depois da primeira instalacao

Depois que tudo estiver configurado, o fluxo diario e curto.

Terminal 1, dependencias:

```bash
docker start officio-sql officio-azurite
```

Terminal 2, backend:

```bash
dotnet run --project src/Officio.WebApi --launch-profile http
```

Terminal 3, frontend:

```bash
cd frontend
npm run dev
```

No PowerShell, use `npm.cmd run dev` caso `npm` seja bloqueado. Para encerrar backend e frontend, pressione `Ctrl+C` nos respectivos terminais. Para parar as dependencias:

```bash
docker stop officio-sql officio-azurite
```

Os dados permanecem nos volumes Docker e voltam na proxima inicializacao.

## 7. Alternativas ao Docker

### SQL Server instalado na maquina

Pode-se usar SQL Server Developer/Express no Windows ou qualquer instancia SQL Server acessivel pela rede. Substitua apenas a connection string dos User Secrets. Exemplo com autenticacao integrada do Windows:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Officio;Trusted_Connection=True;TrustServerCertificate=True" --project src/Officio.WebApi
```

Com usuario e senha do SQL Server, use o mesmo formato do exemplo Docker e altere servidor, porta, usuario e senha. PostgreSQL, MySQL e SQLite nao sao alternativas diretas: a aplicacao usa o provider do Entity Framework Core para SQL Server.

### Conta real do Azure Storage

Em vez do Azurite, informe a connection string de uma conta de armazenamento Azure:

```bash
dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" "<CONNECTION_STRING_DA_CONTA>" --project src/Officio.WebApi
```

### Arquivo `src/.env`

Como alternativa aos User Secrets, copie `src/.envexample` para `src/.env` e preencha as configuracoes. Chaves hierarquicas usam `__`, por exemplo:

```dotenv
ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=Officio;User Id=sa;Password=OfficioDev_2026_Local;Encrypt=False;TrustServerCertificate=True
ConnectionStrings__AzureBlobStorage=UseDevelopmentStorage=true
Jwt__Key=officio-local-only-change-this-key-2026-123456789
```

O arquivo `src/.env` e lido apenas em `Development` e esta ignorado pelo Git. User Secrets, variaveis de ambiente e argumentos de linha de comando prevalecem sobre ele.

## 8. Integracoes opcionais

O sistema basico, incluindo autenticacao, cadastros e persistencia, roda sem credenciais de OCR, Autentique ou SMTP.

| Integracao | Configuracao | Comportamento sem credenciais |
| --- | --- | --- |
| Azure Document Intelligence | `AzureOcr:Endpoint` e `AzureOcr:Key` | OCR fica indisponivel |
| Autentique | `Autentique:ApiToken` e configuracoes relacionadas | envio para assinatura usa o fallback previsto pela aplicacao |
| SMTP | `Email:Smtp:*` | em Development, o conteudo do e-mail e escrito no log |

Configure segredos adicionais pelo mesmo mecanismo, por exemplo:

```bash
dotnet user-secrets set "AzureOcr:Endpoint" "https://<RECURSO>.cognitiveservices.azure.com/" --project src/Officio.WebApi
dotnet user-secrets set "AzureOcr:Key" "<CHAVE>" --project src/Officio.WebApi
dotnet user-secrets set "Autentique:ApiToken" "<TOKEN>" --project src/Officio.WebApi
dotnet user-secrets set "Autentique:WebhookSecret" "<SEGREDO_DO_WEBHOOK>" --project src/Officio.WebApi
```

`Autentique:OrganizationId` e `Autentique:FolderId` sao opcionais. Nunca coloque esses valores em arquivos versionados ou em variaveis `VITE_*`, pois configuracoes Vite sao incorporadas ao bundle publico do navegador.

### Abrir um tunel para o webhook da Autentique

O tunel so e necessario para um servico externo chamar a API local, principalmente durante testes do webhook da Autentique. Ele nao e necessario para usar o frontend local.

Com a API rodando em `http://localhost:5131`, instale e autentique o [ngrok](https://ngrok.com/download) e execute em outro terminal:

```bash
ngrok config add-authtoken <TOKEN_DO_NGROK>
ngrok http 5131
```

O ngrok exibira uma URL HTTPS publica. Configure no provedor a URL completa:

```text
https://<SUBDOMINIO_GERADO>.ngrok-free.app/api/v1/contratos/webhook/autentique
```

Mantenha o terminal do ngrok aberto. Em planos gratuitos, a URL pode mudar quando o tunel e reiniciado; nesse caso, atualize o webhook no provedor. Como alternativa, com o Cloudflare Tunnel instalado, pode-se abrir um tunel temporario com:

```bash
cloudflared tunnel --url http://localhost:5131
```

## 9. Validar a instalacao

Backend:

```bash
dotnet build src/Officio.slnx
dotnet test src/Officio.Testes/Officio.Testes.csproj
```

Frontend:

```bash
cd frontend
npm run build
npm test
```

No PowerShell, substitua `npm` por `npm.cmd` se necessario.

## 10. Problemas comuns

### O container ja existe

Se `docker run` informar que o nome `officio-sql` ou `officio-azurite` ja esta em uso, o container ja foi criado. Inicie-o com `docker start <nome>` em vez de executar `docker run` novamente.

### A API nao conecta ao SQL Server

- confirme `docker ps` e aguarde o SQL Server terminar a inicializacao;
- confira se a senha da connection string e igual a `MSSQL_SA_PASSWORD` usada na primeira criacao do container;
- verifique se a porta `1433` nao esta ocupada por outra instancia;
- uma alteracao posterior de `MSSQL_SA_PASSWORD` no comando nao troca a senha armazenada no volume existente.

### O banco existe, mas nao possui o schema

Para uma instalacao local nova, use um nome de banco ainda inexistente e deixe a API cria-lo. Nao aplique as migrations incrementais sobre uma base vazia. Bancos existentes e compartilhados devem ser atualizados somente com uma estrategia de migracao combinada com a equipe.

### O login nao aceita a nova senha do bootstrap

O bootstrap e idempotente: se o e-mail ja existe, ele nao altera nome, perfil ou senha. Use a senha original, escolha outro e-mail para um ambiente novo ou trate explicitamente os dados locais existentes.

### O frontend nao alcanca a API

- mantenha a API ativa em `http://localhost:5131`;
- confirme que `frontend/.env` possui `API_PROXY_TARGET=http://localhost:5131`;
- reinicie o Vite depois de alterar `.env`;
- deixe `VITE_API_BASE_URL=/api` para usar o proxy local e evitar problemas de CORS.

### Quero usar HTTPS local

Confie no certificado de desenvolvimento e use os perfis HTTPS:

```bash
dotnet dev-certs https --trust
dotnet run --project src/Officio.WebApi --launch-profile https
```

Depois altere `API_PROXY_TARGET` em `frontend/.env` para `https://localhost:7228` e reinicie o frontend.

## Documentacao complementar

- [Backend](src/README.md): arquitetura, projetos e responsabilidades da API.
- [Frontend](frontend/README.md): arquitetura, comportamento e decisoes da interface.
- [Autenticacao local](docs/autenticacao-local.md): detalhes do JWT, bootstrap e tenant logico.
