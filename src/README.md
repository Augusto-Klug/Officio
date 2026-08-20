# Officio

## Arquitetura

- Handlers devem atuar como adaptadores finos: recebem comandos/consultas, montam contratos de entrada e chamam interfaces de servico.
- Regras de negocio, orquestracao de repositorios, acesso ao tenant atual e transicoes de dominio devem ficar em servicos de dominio registrados por interface.
- Novos handlers nao devem injetar repositorios diretamente quando a operacao envolver logica de servico; crie ou reutilize uma interface de servico e mantenha o handler sem regra de negocio.
