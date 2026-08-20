# Contrato a partir de Orcamento Aceito Design

## Objetivo

Implementar a ponte entre orcamento aceito e contrato em rascunho, mantendo o fluxo correto do negocio: o contrato nasce vinculado ao prospecto e o cliente so deve ser criado apos assinatura concluida.

Este bloco nao inclui envio para assinatura, webhook D4Sign, assinatura manual, dashboard, admissao de funcionarios ou automacao de abertura de empresa.

## Contexto

O fluxo de orcamento e catalogo ja permite criar, editar, aceitar e recusar orcamentos. O proximo passo do ciclo comercial e iniciar um contrato a partir de um orcamento aceito.

O dominio atual possui `Contrato.CreateFromBudget(Orcamento budget, Guid clientId, ...)`, mas essa assinatura antecipa a existencia de `Cliente`. Isso contradiz o fluxo de negocio documentado, onde a conversao de prospecto em cliente acontece apos a assinatura do contrato, nao antes.

## Decisao Principal

`Contrato` passa a suportar o estado pre-assinatura com:

- `ProspectId` obrigatorio.
- `ClientId` opcional.
- `BudgetId` obrigatorio quando criado a partir de orcamento.
- `StatusContrato.Rascunho` ao ser criado.

Enquanto o contrato nao estiver assinado, consultas e PDF devem usar dados do `Prospecto`. Quando o contrato for assinado em bloco posterior, o fluxo de assinatura convertera o prospecto em cliente e preenchera `ClientId`.

## Escopo Funcional

### Conversao de Orcamento em Contrato

Criar comando para converter um orcamento aceito em contrato:

- Entrada:
  - `OrcamentoId`
  - `DataInicio`
  - `DataFim` opcional
  - `RenovacaoAutomatica`
  - `ClausulasExtras` opcional
- Saida:
  - `ContratoId`

Regras:

- Orcamento deve existir.
- Orcamento deve estar em `StatusOrcamento.Aceito`.
- Orcamento deve estar vinculado a um prospecto existente.
- Nao deve permitir contrato duplicado para o mesmo orcamento.
- Contrato criado deve copiar tenant, valor total, dia de vencimento e vinculos de origem.
- Handler deve ser fino e chamar uma interface de servico; regra de negocio fica no servico de dominio.

### Consulta de Contratos

Adicionar fluxo de consulta basico:

- Listar contratos do tenant atual.
- Obter contrato por id.

DTO de contrato deve expor:

- `Id`
- `Numero`
- `ProspectoId`
- `ClienteId`
- `OrcamentoId`
- `RazaoSocial`
- `Status`
- `ValorTotal`
- `DiaVencimento`
- `DataInicio`
- `DataFim`
- `RenovacaoAutomatica`
- `ClausulasExtras`
- `PdfGerado`

## API

Manter o padrao atual de rotas com `[action]`.

Endpoints novos:

- `POST /api/v1/orcamentos/ConverterParaContrato/{id}`
- `GET /api/v1/contratos`
- `GET /api/v1/contratos/{id}`

O endpoint de conversao fica em `OrcamentosControlador` porque representa uma transicao do orcamento. As consultas ficam em `ContratosControlador`.

## Arquitetura

### Dominio

- `Contrato` deve ter `ProspectId` e `ClientId?`.
- Criar ou ajustar factory para `Contrato.CriarDeOrcamento(Orcamento orcamento, Prospecto prospecto, ...)`.
- Evitar setters publicos.
- Nao converter prospecto em cliente neste bloco.

### Servicos

Evoluir `IContratoServico` para incluir:

- `ConverterOrcamentoAsync(ConverterOrcamentoContratoRequisicao requisicao, CancellationToken ct)`
- `ListarAsync(CancellationToken ct)`
- `ObterPorIdAsync(Guid contratoId, CancellationToken ct)`

`ContratoServico` deve concentrar:

- Busca do orcamento.
- Validacao de status aceito.
- Verificacao de contrato duplicado.
- Busca do prospecto.
- Criacao e persistencia do contrato.
- Mapeamento para DTOs de resultado.

### Repositorios

Evoluir `IContratoRepositorio` com:

- `Task AdicionarAsync(Contrato contrato, CancellationToken ct = default)`
- `Task<Contrato?> ObterPorOrcamentoAsync(Guid orcamentoId, CancellationToken ct = default)`
- `Task<List<Contrato>> ListarAsync(CancellationToken ct = default)`
- `Task<Contrato?> ObterDetalheAsync(Guid contratoId, CancellationToken ct = default)`

Usar query filters globais para tenant. Nao adicionar filtros manuais de `TenantId`.

### MediatR

Criar comandos/consultas:

- `ConverterOrcamentoParaContratoComando`
- `ListarContratosConsulta`
- `ObterContratoPorIdConsulta`

Handlers devem depender apenas de `IContratoServico`.

### ViewModels

Criar:

- `ConverterOrcamentoParaContratoViewModel`

Controller deve receber ViewModel e converter para command via conversor. Nao enviar ViewModel diretamente ao MediatR.

## Erros

- Orcamento inexistente: falha de negocio com mensagem "Orcamento nao encontrado."
- Orcamento nao aceito: falha com mensagem "Somente orcamentos aceitos podem ser convertidos em contrato."
- Prospecto inexistente: falha com mensagem "Prospecto nao encontrado."
- Contrato duplicado: falha com mensagem "Ja existe contrato para este orcamento."
- Contrato inexistente na consulta por id: falha com mensagem "Contrato nao encontrado."

Controllers devem mapear mensagens de inexistencia para `NotFound` e demais falhas de negocio para `BadRequest`.

## Persistencia

A migration deste bloco deve:

- Adicionar `ProspectId` em `Contracts`.
- Tornar `ClientId` nullable.
- Criar indice unico por tenant/orcamento para impedir duplicidade de contrato por orcamento.
- Manter relacionamentos existentes com `Cliente`, `Orcamento` e novo relacionamento com `Prospecto`.

Nao deve remover dados de contratos existentes sem estrategia explicita. Como o projeto ainda esta em desenvolvimento, se houver contratos legados nos testes/migrations, usar nullable/default compativel para preservar build e migration.

## Testes

Cobertura minima:

- Criar contrato a partir de orcamento aceito.
- Falhar ao converter orcamento em rascunho.
- Falhar ao converter orcamento ja convertido.
- Listar contratos com dados do prospecto.
- Obter contrato por id.
- Handler de contrato depende apenas de `IContratoServico`.
- Controller converte ViewModel para command antes do MediatR.

## Fora do Escopo

- Geracao/revisao visual do PDF de contrato.
- Envio para assinatura.
- Webhook D4Sign.
- Fallback manual.
- Conversao de prospecto em cliente.
- Ativacao do contrato.
- Criacao de notificacoes.

## Decisoes

- Contrato nasce vinculado ao prospecto.
- Cliente so e vinculado depois da assinatura em bloco futuro.
- Conversao fica em endpoint de orcamento; consulta fica em endpoint de contrato.
- Handlers seguem o padrao arquitetural atual: nenhuma regra de negocio e nenhuma injecao direta de repositorio.
- Commits de implementacao devem usar prefixo `AK:`.
