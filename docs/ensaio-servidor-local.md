# Ensaio em servidor local

Este ambiente destina-se a um piloto de baixa concorrencia em uma maquina Windows 11 com 16 GB de RAM. Docker Desktop usa o backend Linux via WSL2, e Docker Compose executa todos os servicos em um unico host; nao e um cluster Kubernetes. O frontend estatico, a API e o SQL Server ficam em containers. Um proxy Nginx serve o frontend e encaminha `/api/*` para a API. Somente `127.0.0.1:8080` e publicado no host; SQL Server e API nao recebem portas publicas.

O Blob Storage e o OCR continuam nos recursos Azure. Mantenha Autentique em sandbox durante o ensaio. Este ambiente nao deve receber dados reais nem ser considerado producao.

## Pre-requisitos

- Docker Desktop atualizado, com backend WSL2 habilitado e iniciado ao entrar no Windows.
- 16 GB de RAM. Limites do Compose: SQL Server 4 GB, API 2 GB e proxy/frontend 256 MB. O restante fica para Windows, WSL2 e os processos auxiliares. Comece com o limite padrao do WSL2 e acompanhe o uso no Docker Desktop/Task Manager antes de aumentar.
- Perfil de energia configurado para nunca suspender o computador durante o piloto. Desligar a tela e aceitavel. Planeje manutencoes e atualizacoes do Windows fora do horario de uso; nao desative atualizacoes de seguranca permanentemente.
- Conta Azure Blob Storage separada para o ensaio, com containers privados. O OCR pode ficar sem credenciais; nesse caso, a validacao OCR fica indisponivel.
- PowerShell aberto na raiz do repositorio e arquivo de ambiente exclusivo para o ensaio.

## Preparar configuracao

```powershell
Copy-Item deploy/servidor-teste.env.example deploy/servidor-teste.env
notepad deploy/servidor-teste.env
```

Preencha `Database__Password`, `Jwt__Key` e `ConnectionStrings__AzureBlobStorage`. Configure as credenciais de integracao conforme os testes planejados. `Acesso__FrontendBaseUrl` deve apontar para a URL publica planejada (`https://officio.app.br`); frontend e API compartilham a mesma origem, portanto CORS nao e necessario. O arquivo real e ignorado pelo Git.

ImageSharp 4 exige chave Six Labors no build Release. Solicite uma [chave comunitaria, se elegivel](https://licensing.sixlabors.com/) ou uma [licenca comercial](https://sixlabors.com/pricing/) e grave o **conteudo completo** da licenca em `deploy/sixlabors.lic`. Inclua os projetos `Officio.Dominio.Servicos` e `Officio.Testes` na solicitacao caso a chave comunitaria seja limitada por assembly. Esse arquivo e ignorado pelo Git e pelo contexto Docker; Compose o fornece apenas como segredo temporario durante `dotnet publish`. Nao adicione a chave ao `.env`, a argumentos de build ou a logs. Sem a licenca, o build Debug funciona, mas o build Release da API falha. Para build Release fora do Docker, defina `$env:SIXLABORS_LICENSE_KEY` com o conteudo completo da licenca ou use `dotnet build -c Release -p:SixLaborsLicenseFile=C:\caminho\para\sixlabors.lic`. Configure tambem o secret `SIXLABORS_LICENSE_KEY` no repositorio GitHub para o workflow `.NET CI`.

## Inicializar o banco e os servicos

Na primeira inicializacao, construa as imagens e crie o schema apenas em um banco vazio:

```powershell
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml up -d sql-server
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml build api web
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml run --rm api --bootstrap-schema
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml up -d api web
```

O bootstrap e somente para banco novo e vazio. Nao o execute sobre uma base com dados. Para atualizacoes posteriores, faca backup e aplique as migrations explicitamente:

```powershell
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml run --rm api --apply-migrations
```

O site fica em `http://localhost:8080`; as chamadas do browser usam `/api/v1` na mesma origem. A API responde apenas dentro da rede do Compose. O SQL Server nao publica a porta 1433.

## Recuperacao e verificacao

Os containers SQL, API e web usam `restart: unless-stopped`: se o processo do container encerrar, Docker tenta reinicia-lo. O health check do SQL testa uma consulta, o da API verifica `/health/live` e o do web verifica o Nginx. Consulte `/health/ready` para verificar a conexao da API com o banco. Compose indica saude e ordena a inicializacao, mas **nao reinicia automaticamente um container apenas por estar marcado como `unhealthy`**.

Para recuperar API ou web cujo processo continue ativo, mas pare de responder, configure `deploy/watchdog-servidor-teste.ps1` no Agendador de Tarefas do Windows **depois** de inicializar os servicos. O watchdog consulta apenas os containers deste Compose, reinicia `api` e `web` quando estao `running` e `unhealthy`, e limita cada reinicio a uma vez a cada 15 minutos. Ele nao reinicia SQL Server: falhas de banco exigem diagnostico, nao reinicios cegos. O estado e o log ficam em `%LOCALAPPDATA%\Officio` da conta que executa a tarefa. Nao usa imagens extras nem expoe o socket do Docker a outro container.

Execute uma vez manualmente para validar o acesso ao Docker Desktop:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Resolve-Path deploy/watchdog-servidor-teste.ps1)
```

No **Agendador de Tarefas**, crie `Officio Watchdog` para executar com a mesma conta Windows que inicia o Docker Desktop, com a opcao **Executar somente quando o usuario estiver conectado**. Configure um gatilho diario que se repete a cada **1 minuto**, por tempo **indefinido**; em **Acoes**, use `powershell.exe` e o argumento `-NoProfile -ExecutionPolicy Bypass -File "C:\caminho\completo\Officio\deploy\watchdog-servidor-teste.ps1"`. Em **Configuracoes**, escolha **Nao iniciar uma nova instancia** quando a tarefa anterior ainda estiver ativa. Teste **Executar** no Agendador e confira o ultimo resultado e `%LOCALAPPDATA%\Officio\watchdog.log`. A tarefa nao deve ser instalada se o arquivo real `deploy/servidor-teste.env` ainda nao existir. Se o Docker Desktop/WSL2 ou o Windows pararem, este watchdog nao consegue agir; e necessario monitoramento externo para detectar essa indisponibilidade.

```powershell
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml ps
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml logs -f api
docker stats
```

Para validar a recuperacao de encerramento da API:

```powershell
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml kill api
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml ps
```

Confirme que o container voltou a `running` e que `http://localhost:8080` e a API respondem. Para uma reinicializacao inesperada do Windows, o Docker Desktop precisa iniciar novamente; habilite **Start Docker Desktop when you sign in** e confira que os containers voltam apos o Engine iniciar. A politica Docker nao evita indisponibilidade durante falha do Windows, WSL2, energia, rede ou do proprio disco.

## Publicacao externa

O proxy escuta no host somente em `127.0.0.1:8080`. Quando a zona do dominio e o Named Tunnel estiverem prontos, configure o origin do tunnel como `http://localhost:8080`. O tunnel publica o frontend e a API na mesma origem e mantem as portas do SQL/API fechadas no roteador. O callback Autentique fica em `https://officio.app.br/api/v1/contratos/webhook/autentique`. Antes de divulgar o endereco, valide login, links de convite, upload/download do Blob e o fluxo Autentique sandbox por HTTPS externo.

Nao use Quick Tunnel como URL permanente: ele gera hostname temporario. Nao coloque tokens, connection strings ou segredos no frontend ou no bundle Vite.

## Parar e preservar dados

```powershell
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml down
```

Os dados permanecem no volume nomeado `officio-servidor-teste-sql`. Nao use `down -v` durante o ensaio: isso remove o banco local. Mantenha backups fora do volume e teste restauracao antes de depender desses dados.

## Limite entre ensaio e producao

SQL Server Developer e licenciado somente para desenvolvimento e teste. Nao transforme este banco em producao apenas apagando os registros. Para uso real, crie banco e credenciais separados, escolha edicao/licenca adequada (por exemplo, avaliar SQL Server Express dentro do limite de 10 GB ou um servico gerenciado), configure backups/restauracao e migre somente os dados autorizados pelo parceiro.
