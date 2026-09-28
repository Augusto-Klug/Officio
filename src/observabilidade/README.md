# Observabilidade local

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

Reinicie a API depois de iniciar a stack. O Collector encaminha traces ao Jaeger e metricas ao Prometheus; o Grafana consulta os dois.

## Interfaces

- Grafana: http://localhost:3000 (usuario `admin`, senha local inicial `admin`)
- Dashboard: pasta `Officio`, dashboard `Officio - Observabilidade`
- Jaeger: http://localhost:16686
- Prometheus: http://localhost:9090
- Alertmanager: http://localhost:9093
- Caixa de e-mail local (Mailpit): http://localhost:8025

As portas publicadas estao limitadas a `127.0.0.1`. Troque a senha local do Grafana apos o primeiro acesso, se necessario.

Alertmanager aplica regras de indisponibilidade do Collector, taxa de erros 5xx e latencia HTTP, alem de fila cheia, falhas de processamento, acumulo e assinaturas invalidas dos webhooks Autentique. O dashboard tambem exibe a quantidade de alertas ativos.

As notificacoes sao direcionadas ao Mailpit local e nao sao entregues a destinatarios reais. Para testar o roteamento, envie um alerta sintetico ao Alertmanager:

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

Depois de alguns segundos, a mensagem aparece no Mailpit. Jaeger usa armazenamento em memoria e perde traces quando reinicia; Prometheus conserva metricas por sete dias em volume Docker.

## Logs e encerramento

```powershell
docker compose -f docker-compose.observabilidade.yml logs -f otel-collector
docker compose -f docker-compose.observabilidade.yml down
```

`down` preserva os volumes de metricas e configuracao do Grafana. Nao use `down -v` se quiser manter esses dados.
