# Officio

## Arquitetura

- Handlers devem atuar como adaptadores finos: recebem comandos/consultas, montam contratos de entrada e chamam interfaces de servico.
- Regras de negocio, orquestracao de repositorios, acesso ao tenant atual e transicoes de dominio devem ficar em servicos de dominio registrados por interface.
- Novos handlers nao devem injetar repositorios diretamente quando a operacao envolver logica de servico; crie ou reutilize uma interface de servico e mantenha o handler sem regra de negocio.

## Regra para handlers

Quando o input vindo do front tiver 3 ou mais parametros, a WebApi deve converter esse input em um contrato de requisicao/filtro antes de enviar ao MediatR. O handler deve receber o contrato dentro do command/query, nao uma lista longa de parametros soltos. O conversor deve ter interface propria e registro no DI da WebApi.
