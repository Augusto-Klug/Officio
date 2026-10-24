# Observabilidade local e piloto

## Iniciar

Na raiz do repositorio:

```powershell
docker compose -f docker-compose.observabilidade.yml up -d
```

Configure a API com:

```text
Observabilidade__Otlp__Endpoint=http://localhost:4317
Observabilidade__TraceSampleRatio=1.0
```

Reinicie a API depois de iniciar a stack. O `ILogger` e exportado via OTLP junto com traces e metricas. O Collector encaminha traces ao Jaeger, metricas ao Prometheus e logs ao Loki; o Grafana consulta as tres fontes. O Console da API continua ativo em JSON para `docker compose logs`.

## Piloto Docker

O stack e opcional e nao altera o Compose normal do piloto. Para iniciar API, web, SQL e observabilidade juntos:

```powershell
docker compose --env-file deploy/servidor-teste.env -f docker-compose.servidor-teste.yml -f docker-compose.observabilidade.yml -f docker-compose.servidor-teste.observabilidade.yml up -d --build
```

O overlay configura a API para `http://otel-collector:4317` pela rede interna do Compose. Nao use `localhost:4317` dentro do container da API. O collector, Loki e banco nao ficam expostos publicamente; as interfaces de Grafana, Jaeger, Prometheus, Alertmanager e Mailpit ficam presas a `127.0.0.1`.

Limites de memoria configurados para o piloto: SQL 2,5 GiB (SQL Server limitado a 2048 MiB), API 2 GiB, web 256 MiB, Collector 192 MiB, Jaeger 384 MiB, Prometheus 384 MiB, Loki 384 MiB, Grafana 256 MiB, Alertmanager 96 MiB e Mailpit 96 MiB. O limite agregado e aproximadamente 6,5 GiB. O SQL Server fica acima do minimo documentado de 2 GiB e ainda tem 512 MiB de folga dentro do limite do container. E uma margem superior configurada, nao consumo esperado; confira `docker stats` durante uso real e ajuste se a VM/WSL tiver limite menor que 8 GiB. Os logs JSON dos containers rotacionam entre 10 e 20 MiB, com 2 ou 3 arquivos por container.

Loki mantem dados por sete dias em volume Docker persistente e nao publica a porta 3100. Grafana e os logs contem dados operacionais: troque a senha inicial `admin/admin`, mantenha os paineis acessiveis apenas localmente e nunca encaminhe a porta 3100 para a internet. O Loki deste setup e single-node para piloto, sem HA; perda do volume significa perda dos logs.

## Interfaces

- Grafana: http://localhost:3000 (usuario `admin`, senha local inicial `admin`)
- Dashboard: pasta `Officio`, dashboard `Officio - Observabilidade`
- Jaeger: http://localhost:16686
- Prometheus: http://localhost:9090
- Logs: Grafana > Explore > datasource Loki, ou dashboard `Officio - Observabilidade`
- Alertmanager: http://localhost:9093
- Caixa de e-mail local (Mailpit): http://localhost:8025

As portas publicadas estao limitadas a `127.0.0.1`. Troque a senha local do Grafana apos o primeiro acesso, se necessario.

Alertmanager aplica regras de indisponibilidade do Collector, taxa de erros 5xx e latencia HTTP, alem de fila cheia, falhas de processamento, acumulo e assinaturas invalidas dos webhooks Autentique. O dashboard tambem exibe a quantidade de alertas ativos e logs recentes da API.

As notificacoes sao direcionadas ao Mailpit local e nao sao entregues a destinatarios reais. Em piloto, nao ha destino operacional de alerta configurado; Mailpit e apenas uma caixa local de desenvolvimento. Para testar o roteamento, envie um alerta sintetico ao Alertmanager:

```powershell
$agora = [DateTime]::UtcNow
$alerta = @(@{
    labels = @{ alertname = 'TesteNotificacaoOfficio'; severity = 'warning' }
    annotations = @{ summary = 'Teste de notificacao'; description = 'Alerta sintetico local.' }
    startsAt = $agora.ToString('o')
    endsAt = $agora.AddMinutes(5).ToString('o')
})
Invoke-RestMethod -Uri http://localhost:9093/api/v2/alerts -Method Post -ContentType 'application/json' -Body (ConvertTo-Json -InputObject $alerta -Depth 5)
```

Depois de alguns segundos, a mensagem aparece no Mailpit. Jaeger usa armazenamento em memoria e perde traces quando reinicia; Prometheus conserva metricas por sete dias e Loki conserva logs por sete dias em volumes Docker.

## Logs e encerramento

```powershell
docker compose -f docker-compose.observabilidade.yml logs -f otel-collector
docker compose -f docker-compose.observabilidade.yml down
```

`down` preserva os volumes de metricas e configuracao do Grafana. Nao use `down -v` se quiser manter esses dados.
