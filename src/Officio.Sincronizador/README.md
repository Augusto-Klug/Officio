# Officio.Sincronizador

Executavel de operacoes agendadas do Officio. Reprocessa assinaturas pendentes da Autentique e encerra contratos ativos cuja data final ja passou, usando a data local de `America/Sao_Paulo`.

## Execucao manual

Na raiz do repositorio, com o `.env` local configurado:

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project .\src\Officio.Sincronizador\Officio.Sincronizador.csproj
```

O projeto usa `ConnectionStrings__DefaultConnection`, `Database__Password`, `Autentique__BaseUrl`, `Autentique__ApiToken` e `ConnectionStrings__AzureBlobStorage`. Em `Development`, carrega o `.env` encontrado na pasta atual ou em uma pasta ancestral. Variaveis do processo prevalecem sobre o arquivo.

Para publicar atualizacoes SignalR depois de reconciliar uma assinatura, configure tambem `TempoReal__CallbackUrl` como URL base da aplicacao que encaminha `/api/` para a WebAPI e `TempoReal__ChaveCompartilhada` com um segredo aleatorio compartilhado com a API. O cliente HTTP acrescenta `/api/v1/interno/atualizacoes-tempo-real` a URL base. No deploy atual, o sincronizador roda como servico systemd no host e o Nginx `web` publica `127.0.0.1:8080`, encaminhando `/api/` para `api:8080`; portanto, use `TempoReal__CallbackUrl=http://127.0.0.1:8080` (nao `http://api:8080`, que nao resolve no host). Configure o mesmo segredo na API. Nao reutilize tokens de usuario nem versione o segredo. O callback tem timeout de 5 segundos e falhas de publicacao sao registradas como warning sem interromper o processamento do job.

## Agendamento no Linux

Publique o projeto para Linux e instale os arquivos `deploy/systemd` no host. Configure o fuso do sistema para `America/Sao_Paulo` para que o timer rode diariamente as 07h10 locais.

```bash
dotnet publish src/Officio.Sincronizador/Officio.Sincronizador.csproj -c Release -o /opt/officio
sudo install -m 0644 src/Officio.Sincronizador/deploy/systemd/officio-sincronizador.service /etc/systemd/system/
sudo install -m 0644 src/Officio.Sincronizador/deploy/systemd/officio-sincronizador.timer /etc/systemd/system/
sudo timedatectl set-timezone America/Sao_Paulo
sudo systemctl daemon-reload
sudo systemctl enable --now officio-sincronizador.timer
```

Crie o usuario/grupo `officio` e `/etc/officio/officio.env` com as mesmas variaveis de configuracao, no formato `NOME=valor`. Mantenha esse arquivo acessivel apenas ao usuario do servico. O host precisa ter o runtime .NET 10 e o SQL/Blob precisam estar acessiveis com as configuracoes fornecidas.

Para executar manualmente no servidor e conferir o resultado:

```bash
sudo systemctl start officio-sincronizador.service
sudo journalctl -u officio-sincronizador.service -n 100 --no-pager
```

O timer executa as duas rotinas uma vez por dia. Contratos permanecem validos durante toda a `DataFim` e sao encerrados quando o job roda no dia seguinte. A automacao que inicia e desaloca a VM e configurada separadamente pelo Azure.
