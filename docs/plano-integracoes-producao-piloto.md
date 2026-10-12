# Plano de preparacao para o piloto

## Arquitetura acordada

- Um host Windows 11 de 16 GB, com Docker Desktop usando WSL2 e Docker Compose. E uma instalacao de host unico, nao um cluster Kubernetes.
- O container `web` usa Nginx para servir o frontend Vue e encaminhar `/api/*` para a API. O browser usa `/api/v1`; as rotas dos controllers continuam inalteradas.
- A API e o SQL Server ficam somente na rede interna do Compose. Apenas o proxy e publicado em `127.0.0.1:8080`; o SQL nao tem porta publicada.
- O volume nomeado do SQL persiste os dados entre recriacoes dos containers. Backups externos e teste de restauracao continuam obrigatorios.
- O Cloudflare Named Tunnel deve encaminhar o hostname publico para `http://localhost:8080`. O dominio serve frontend, API e callback Autentique na mesma origem.
- Blob Storage continua no Azure; OCR pode usar o plano gratuito durante os testes. Autentique permanece em sandbox ate haver autorizacao para credenciais/assinaturas reais.
- O banco SQL Server Developer e apenas para desenvolvimento/ensaio, nunca para producao com dados reais.

## Recuperacao e limites

- `restart: unless-stopped` reinicia containers quando o processo encerra e o Engine esta ativo.
- Health checks verificam SQL, conectividade API-banco e Nginx. Em Compose, o estado `unhealthy` e diagnostico/ordenacao; ele nao reinicia sozinho um processo que continua ativo mas travado.
- Uma replica de API ou um cluster K3s no mesmo host nao recupera falha do Windows, WSL2, energia, rede ou SQL. Para o piloto, o custo operacional do Kubernetes nao se justifica.
- O perfil de energia deve impedir suspensao. Habilite o inicio do Docker Desktop ao entrar no Windows e teste retorno apos reinicio. Reinicios inesperados e atualizacoes ainda podem causar indisponibilidade.
- O watchdog PowerShell ja foi implementado para reiniciar API e frontend quando permanecerem `unhealthy`, com limite de uma tentativa por servico a cada 15 minutos. Ele nao reinicia o SQL Server; a configuracao no Agendador de Tarefas e a validacao no host ainda precisam ser feitas.

## Etapas e criterios de saida

### 1. Servir frontend e API no mesmo host - implementacao inicial

- Construir frontend estatico em imagem e servir pelo Nginx.
- Encaminhar `/api/*` ao container API e preservar a URL `/api/v1` do backend.
- Manter API/SQL sem portas no host; publicar proxy somente em loopback para o tunnel.
- Configurar health checks, politica de reinicio e volume persistente do SQL.
- Validar build das imagens, migrations em banco de ensaio, login e navegacao SPA.
- Saida: frontend local em `http://localhost:8080`, endpoints em mesma origem e SQL inacessivel pelo host/rede externa.

### 2. Preparar Windows 11 e testar recuperacao

- Habilitar Docker Desktop com backend WSL2 e inicializacao ao entrar no Windows.
- Manter suspensao/hibernacao desabilitadas durante o piloto; planejar manutencoes fora do horario de uso.
- Observar o consumo real com `docker stats` e Task Manager; limites atuais: SQL 4 GB, API 2 GB e web 256 MB.
- Testar encerramento forcado da API e confirmar que Docker reinicia o container. Reiniciar o host em janela controlada e conferir retorno do Docker Desktop/containers.
- Registrar que health check `unhealthy` nao gera reinicio automatico em Compose e configurar o watchdog versionado no Agendador de Tarefas.
- Testar encerramento forcado de API e frontend, estado `unhealthy`, limite de tentativas e retorno apos reinicio do host.
- Saida: recuperacao dos processos documentada; validacao da tarefa agendada e do retorno do host deve ser concluida no servidor de ensaio, sem assumir alta disponibilidade do host.

### 3. Configurar dominio e Cloudflare Named Tunnel

- Adicionar `officio.app.br` a zona Cloudflare, delegar nameservers no registrador e confirmar zona `Active`, preservando registros existentes.
- Criar Named Tunnel e encaminhar `officio.app.br` para `http://localhost:8080` no host.
- Nao abrir portas 1433, 8080 ou API no roteador/firewall. Confirmar acesso externo somente via HTTPS do Cloudflare.
- Configurar `Acesso__FrontendBaseUrl=https://officio.app.br` e cadastrar webhook Autentique como `https://officio.app.br/api/v1/contratos/webhook/autentique`.
- Saida: frontend, API e webhook acessiveis externamente pelo dominio; API e SQL sem exposicao direta.

### 4. Ensaiar integracoes Azure e Autentique

- Criar/usar uma conta Blob Storage separada de desenvolvimento, com containers privados. Testar upload, download, exclusao e PDFs.
- Configurar Document Intelligence F0 somente para os documentos OCR previstos; sem credenciais, registrar OCR indisponivel.
- Manter Autentique em sandbox; testar assinatura, eventos duplicados, HMAC, inbox e reconciliacao pelo `Officio.Sincronizador`.
- Validar convite, redefinicao de senha e SMTP real quando houver credenciais configuradas; manter segredos fora do Git.
- Saida: fluxos completos validados pela URL HTTPS publica, sem depender do Azurite ou de Quick Tunnel.

### 5. Preparar producao real

- Escolher ambiente de execucao apropriado. Windows 11 + Docker Desktop e alvo de piloto, nao uma garantia de disponibilidade para producao.
- Criar SQL/database de producao separado com edicao/licenca adequada, backups, retencao e procedimento de restauracao testado.
- Criar credenciais independentes de producao para Azure Blob/OCR, Autentique e SMTP; manter sandbox e ensaio isolados.
- Revisar monitoramento, alertas, logs sem PII/segredos, atualizacoes e processo de resposta a indisponibilidade.
- Executar testes autorizados de assinatura real e aprovar checklist de go-live antes de convidar o parceiro.

## Configuracao necessaria no host

Na raiz do repositorio, preparar `deploy/servidor-teste.env` a partir do exemplo. Configure senha do banco, chave JWT, Azure Blob, credenciais Autentique sandbox e SMTP conforme os testes. Para mesma origem, CORS nao e necessario; links de convite/redefinicao devem usar `https://officio.app.br` quando o dominio estiver ativo.

No Docker Desktop, habilite o backend WSL2 e inicializacao ao entrar no Windows. Comece com o limite de memoria padrao do WSL2 e acompanhe uso; o padrao e ate 50% da RAM fisica para a VM WSL2. Com 16 GB, isso representa ate 8 GB, portanto os limites do Compose foram reduzidos para deixar margem ao sistema Linux. Ajuste WSL somente com medicao e mantendo memoria suficiente para Windows.

## Quando o dominio fica pronto

O dominio pode ser delegado antes do deploy. A URL `https://officio.app.br` fica pronta quando a zona esta ativa, o Named Tunnel esta conectado ao origin `http://localhost:8080`, o proxy/web/API estao saudaveis e os testes HTTPS externos passam. Somente entao atualize os links de convite e o callback Autentique.
