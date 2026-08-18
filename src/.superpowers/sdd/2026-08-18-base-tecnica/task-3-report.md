# Task 3: Eventos de Dominio Despachados Apos Persistencia

## Status

Concluida. O codigo existente ja atendia ao fluxo exigido:

- coleta `EntidadeBase.EventosDominio` antes de persistir;
- executa `base.SaveChangesAsync(ct)` antes da publicacao;
- publica cada evento via `IPublisher.Publish(domainEvent, ct)`;
- limpa os eventos das entidades rastreadas apos a publicacao.

## Alteracoes

Adicionado o teste `SaveChangesAsync_QuandoEntidadePossuiEvento_PublicaEventoELimpaFila`, cobrindo a conversao de um prospecto, a publicacao de uma notificacao e a limpeza da fila de eventos no mesmo fluxo.

Nenhuma alteracao foi feita nos pontos das Tasks 1 e 2. O fake de publicacao existente ja implementava o registro das notificacoes.

## Testes

- `dotnet test Officio.Testes/Officio.Testes.csproj --filter ContextoAplicacaoEventosTestes`: passou, 3/3.
- `dotnet test Officio.Testes/Officio.Testes.csproj --filter "ContextoAplicacaoEventosTestes|ContextoAplicacao"`: passou, 3/3, 0 falhas, duracao de 574 ms.
- `dotnet test Officio.Testes/Officio.Testes.csproj --no-build --filter "ContextoAplicacaoEventosTestes|ContextoAplicacao"`: passou, 3/3.
- `git diff --check`: passou.

## Concerns

Nenhum concern funcional identificado.
