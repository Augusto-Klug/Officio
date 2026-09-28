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

As portas publicadas estao limitadas a `127.0.0.1`. Troque a senha local do Grafana apos o primeiro acesso, se necessario.

Jaeger usa armazenamento em memoria e perde os traces quando reinicia. Prometheus conserva metricas por sete dias em volume Docker. O dashboard de webhooks so tera dados depois que a API exportar medicoes desse fluxo.

## Logs e encerramento

```powershell
docker compose -f docker-compose.observabilidade.yml logs -f otel-collector
docker compose -f docker-compose.observabilidade.yml down
```

`down` preserva os volumes de metricas e configuracao do Grafana. Nao use `down -v` se quiser manter esses dados.
