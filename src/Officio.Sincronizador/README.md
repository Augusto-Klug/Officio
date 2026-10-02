# Officio.Sincronizador

Executavel de operacoes agendadas do Officio. No momento executa a reconciliacao das assinaturas da Autentique uma vez e encerra com codigo diferente de zero quando houver falhas.

## Execucao manual

Na raiz do repositorio, com o `.env` local configurado:

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project .\src\Officio.Sincronizador\Officio.Sincronizador.csproj
```

O projeto usa `ConnectionStrings__DefaultConnection`, `Database__Password`, `Autentique__BaseUrl`, `Autentique__ApiToken` e `ConnectionStrings__AzureBlobStorage`. Em `Development`, carrega o `.env` encontrado na pasta atual ou em uma pasta ancestral. Variaveis do processo prevalecem sobre o arquivo.

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

O timer executa o job uma vez por dia. A automacao que inicia e desaloca a VM e configurada separadamente pelo Azure.
