# Piloto Azure de uma semana

Este roteiro prepara um ambiente isolado para um parceiro, com ativacao em data ainda a combinar. Os arquivos de build neste repositorio nao provisionam recursos nem publicam a aplicacao. A hospedagem planejada e Azure Static Web Apps Free (frontend), Azure Container Apps Consumption (API), Azure SQL Database Free/serverless (banco), Blob Storage Hot LRS (arquivos) e Document Intelligence F0 (OCR). A Autentique e o SMTP continuam sendo integracoes externas.

## Preparar agora

1. Construir a API no contexto da raiz do repositorio: `docker build -f src/Officio.WebApi/Dockerfile -t officio-api:piloto .`.
2. Construir o frontend com `VITE_API_BASE_URL=https://<host-da-api>/api` no ambiente do processo que executa `npm run build` em `frontend/`. Esse valor entra no JavaScript publico durante o build; nao colocar segredos em variaveis `VITE_*`.
3. Registrar um nome de imagem e uma versao imutavel para cada implantacao; guardar a versao anterior para eventual retorno.
4. Escolher assinatura e regiao Azure, confirmar elegibilidade das ofertas gratuitas e estimar o volume de documentos em paginas. O F0 de OCR tem limite mensal, nao uma franquia por arquivo.
5. Definir o endereco publico do frontend e o da API antes de distribuir convites. Configurar `Acesso__FrontendBaseUrl` com a origem do frontend e `Cors__AllowedOrigins__0` com a mesma origem. O navegador usa a URL absoluta da API definida em `VITE_API_BASE_URL`; CORS nao interfere no webhook enviado pela Autentique.

O `staticwebapp.config.json` do frontend preserva rotas Vue ao recarregar paginas internas. O container da API ouve HTTP na porta 8080; o HTTPS publico termina no ingresso do Container Apps. O arquivo `src/.env` e os artefatos locais estao excluidos do contexto Docker.

## Configuracao do ambiente

| Recurso | Configuracao inicial para o piloto |
| --- | --- |
| Static Web Apps | Plano Free, build de `frontend/` para `dist/`; usar o mesmo host em `Acesso__FrontendBaseUrl` e CORS. |
| Container Apps | Plano Consumption, ingresso externo HTTPS para a porta 8080, 0,5 vCPU e 1 GiB de memoria. Uma replica minima facilita o processamento imediato dos webhooks; a inbox SQL preserva eventos durante reinicios. |
| Azure SQL | Criar banco novo pela oferta gratuita, se disponivel, General Purpose serverless, minimo de 0,5 vCore, auto-pause de 15 minutos e limite de 32 GB. Monitorar vCore-segundos restantes; a opcao de parar ao esgotar a cota deixa a base indisponivel ate o mes seguinte. |
| Blob Storage | Conta GPv2 com redundancia LRS, acesso Hot e containers privados. Confirmar que o upload e o download de PDFs e documentos continuam funcionando. |
| Document Intelligence | Recurso F0, dentro do limite de 500 paginas analisadas no mes. Confirmar disponibilidade da oferta na assinatura. |
| Custos | Criar budget e alertas para a assinatura/grupo de recursos. Alertas avisam sobre gastos, mas nao os interrompem. Ingestao de logs, trafego de saida, dominio, SMTP e assinatura da Autentique precisam ser avaliados separadamente. |

Segredos da API devem entrar como referencias a secrets do Container Apps: `ConnectionStrings__DefaultConnection`, `ConnectionStrings__AzureBlobStorage`, `Jwt__Key`, `AzureOcr__Key`, `Autentique__ApiToken`, `Autentique__WebhookSecret` e `Email__Smtp__Password`. Configuracoes nao secretas incluem `ASPNETCORE_ENVIRONMENT=Production`, `AzureOcr__Endpoint`, `Autentique__Sandbox`, `Acesso__FrontendBaseUrl`, `Cors__AllowedOrigins__0`, host/porta/remetente SMTP e, se necessario, `Email__Smtp__User`. Nao copiar `src/.env` ou senhas de desenvolvimento para a imagem ou configuracao publica.

## Bloqueios antes de convidar o parceiro

- **Primeiro administrador:** `BootstrapUsuario` so funciona em `Development`. Definir e testar uma rotina explicita, executada uma unica vez em ambiente de piloto, para criar o PlatformAdmin inicial sem habilitar `Development` na API publica. Depois, criar o parceiro e enviar o convite ao administrador pela rota existente.
- **SQL apos pausa:** testar a primeira requisicao depois do auto-resume. A aplicacao ainda precisa de estrategia testada para erros transitorios de conexao nesse momento; monitorar tambem conexoes que impeçam o banco de pausar.
- **Banco inicial:** em um banco realmente vazio, executar a imagem da API com `--bootstrap-schema` e as mesmas configuracoes de banco/JWT. Para atualizacoes posteriores, usar `--apply-migrations` apos backup. A inicializacao normal fora de `Development` nao cria schema nem aplica migrations.
- **Integracoes:** testar convites por SMTP, callback HTTPS com assinatura do webhook, geracao e leitura de PDFs no Blob, OCR F0, autorizacao e formulario publico de admissao. Repetir o teste de webhook depois de deixar a API ociosa.

### Inbox de webhooks Autentique

A API valida a assinatura HMAC e grava o payload em `AutentiqueWebhookInbox` antes de responder `202`. O hash SHA-256 tem indice unico para que reenvios identicos sejam aceitos sem criar outra tarefa. O worker reserva os eventos no SQL, recupera leases expirados e tenta processar novamente com atraso crescente, ate oito tentativas. Payloads concluidos sao apagados da inbox; os hashes permanecem para deduplicacao. Falhas finais ficam com `Status = 3` e `UltimoErro`.

Antes do onboarding, validar no staging que o evento continua pendente/processavel depois de reiniciar a API, que uma entrega duplicada nao duplica efeitos e que indisponibilidade do SQL retorna `503` para a Autentique tentar novamente. Conferir falhas finais com:

```sql
SELECT Id, HashPayload, RecebidoEm, Tentativas, UltimoErro
FROM dbo.AutentiqueWebhookInbox
WHERE Status = 3
ORDER BY RecebidoEm DESC;
```

Depois de corrigir a causa de uma falha, um operador pode reenfileirar pelo `Id`:

```sql
UPDATE dbo.AutentiqueWebhookInbox
SET Status = 0,
    Tentativas = 0,
    ProximaTentativaEm = SYSUTCDATETIME(),
    BloqueadoAte = NULL,
    UltimoErro = NULL
WHERE Id = '<id-do-evento>'
  AND Status = 3;
```

## Semana de ativacao

1. Confirmar com o parceiro a data, e-mail do administrador, CNPJ, lista de usuarios iniciais e quais dados reais entrarao no sistema.
2. Provisionar os recursos em grupo dedicado, configurar secrets e ingressos, criar o banco e executar o bootstrap do schema. Validar as configuracoes antes de ligar o frontend publico.
3. Concluir os bloqueios acima em ambiente isolado, executar um fluxo completo com dados de teste e registrar versoes da imagem/API/frontend.
4. Criar o primeiro administrador pela rotina de producao validada, cadastrar o parceiro pela API de administracao e enviar o convite ao administrador. Confirmar login e permissao do primeiro usuario.
5. Durante os sete dias, conferir diariamente falhas de webhook, uso do SQL Free, paginas de OCR e gastos. Preservar a imagem anterior; testar restauracao do banco dentro da janela de PITR disponivel.
6. Ao fim do piloto, decidir junto ao parceiro a continuidade e o destino dos dados. Desligar recursos que nao forem mais usados; apenas pausar o frontend/API nao interrompe necessariamente a cobranca de armazenamento.

Referencias: [Azure SQL Free](https://learn.microsoft.com/en-us/azure/azure-sql/database/free-offer), [Container Apps Consumption](https://learn.microsoft.com/en-us/azure/container-apps/billing), [Static Web Apps Free](https://azure.microsoft.com/en-us/pricing/details/app-service/static/), [Document Intelligence F0](https://azure.microsoft.com/en-us/pricing/details/document-intelligence/) e [budgets Azure](https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/tutorial-acm-create-budgets).
