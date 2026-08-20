# Contrato a partir de Orcamento Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Criar o fluxo que converte um orcamento aceito em contrato rascunho vinculado ao prospecto, sem converter cliente antes da assinatura.

**Architecture:** `Contrato` passa a suportar `ProspectId` obrigatorio e `ClientId` opcional. Handlers MediatR continuam finos e chamam `IContratoServico`; o servico concentra validacoes, repositorios e mapeamento de resultados. A persistencia usa EF Core, query filters globais de tenant e indice unico para impedir contrato duplicado por orcamento.

**Tech Stack:** .NET 10, ASP.NET Core Web API, EF Core SQL Server, MediatR 14, FluentValidation, xUnit.

**Spec:** `docs/superpowers/specs/2026-08-20-contrato-a-partir-orcamento-design.md`

## Global Constraints

- Contrato nasce vinculado ao prospecto.
- Cliente so e vinculado depois da assinatura em bloco futuro.
- Conversao fica em endpoint de orcamento; consulta fica em endpoint de contrato.
- Handlers seguem o padrao arquitetural atual: nenhuma regra de negocio e nenhuma injecao direta de repositorio.
- Controllers recebem ViewModels e convertem para Commands; nunca enviar ViewModel diretamente ao MediatR.
- Multi-tenancy deve usar query filters globais; nao adicionar filtro manual de `TenantId`.
- Nao adicionar setters publicos em entidades de dominio.
- D4Sign, webhook, fallback manual, ativacao de contrato e conversao de cliente ficam fora deste bloco.
- Commits de implementacao devem usar prefixo `AK:`.

---

## File Structure

- `Officio.Dominio/Entidades/Contrato.cs`: adiciona `ProspectId`, torna `ClientId` opcional e cria factory `CriarDeOrcamento`.
- `Officio.Dominio/Contratos/ContratosContratos.cs`: adiciona contratos de entrada/saida para conversao e consultas.
- `Officio.Dominio/Interfaces/IContratoServico.cs`: adiciona metodos de conversao/listagem/detalhe.
- `Officio.Dominio/Interfaces/IContratoRepositorio.cs`: adiciona persistencia e consultas basicas de contrato.
- `Officio.Dominio.Servicos/Servicos/ContratoServico.cs`: implementa conversao, listagem e detalhe.
- `Officio.Infra.BancoDeDados/Repositorios/ContratoRepositorio.cs`: implementa consultas EF.
- `Officio.Infra.BancoDeDados/Mapeamentos/ContratoMap.cs`: mapeia `ProspectId`, `ClientId?`, relacoes e indice unico por orcamento.
- `Officio.Dominio.Mediador/Funcionalidades/Contratos/...`: comandos/consultas/handlers para conversao e consulta.
- `Officio.ViewModels/Contratos/ConverterOrcamentoParaContratoViewModel.cs`: entrada HTTP da conversao.
- `Officio.WebApi/Conversores/ContratoViewModelConversor.cs`: conversao ViewModel -> Command.
- `Officio.WebApi/Controllers/OrcamentosControlador.cs`: endpoint de conversao.
- `Officio.WebApi/Controllers/ContratosControlador.cs`: endpoints de listagem e detalhe.
- `Officio.Infra.BancoDeDados/Migrations/*`: migration do schema de contratos.
- `Officio.Testes/Unitarios/...`: testes de dominio, servico, handler, controller/conversor e repositorio.

---

### Task 1: Modelo de Contrato Pre-Assinatura

**Files:**
- Modify: `Officio.Dominio/Entidades/Contrato.cs`
- Test: `Officio.Testes/Unitarios/Dominio/ContratoTestes.cs`

**Interfaces:**
- Produces: `Contrato.CriarDeOrcamento(Orcamento orcamento, Prospecto prospecto, DateTime dataInicio, DateTime? dataFim, bool renovacaoAutomatica, string? clausulasExtras)`.
- Produces: `Contrato.ProspectId`, `Contrato.ClientId?`, `Contrato.BudgetId`.

- [ ] **Step 1: Write failing domain tests**

Create or extend `Officio.Testes/Unitarios/Dominio/ContratoTestes.cs`:

```csharp
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;

namespace Officio.Testes.Unitarios.Dominio;

public class ContratoTestes
{
    [Fact]
    public void CriarDeOrcamento_ComOrcamentoAceito_CriaContratoRascunhoVinculadoAoProspecto()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Honorarios mensais",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            1,
            500m,
            1));
        orcamento.Aceitar();

        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            new DateTime(2026, 9, 1),
            null,
            true,
            "Clausula extra");

        Assert.Equal(tenantId, contrato.TenantId);
        Assert.Equal(prospecto.Id, contrato.ProspectId);
        Assert.Null(contrato.ClientId);
        Assert.Equal(orcamento.Id, contrato.BudgetId);
        Assert.Equal(StatusContrato.Rascunho, contrato.Status);
        Assert.Equal(500m, contrato.ValorTotal);
        Assert.Equal(10, contrato.DiaVencimento);
        Assert.True(contrato.RenovacaoAutomatica);
        Assert.Equal("Clausula extra", contrato.ClausulasExtras);
        Assert.StartsWith("CTR-", contrato.Numero);
    }

    [Fact]
    public void CriarDeOrcamento_ComProspectoDiferente_LancaErro()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var outroProspecto = Prospecto.Create(tenantId, "Outra Empresa", "55666777000188", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.Aceitar();

        var erro = Assert.Throws<InvalidOperationException>(() =>
            Contrato.CriarDeOrcamento(orcamento, outroProspecto, DateTime.UtcNow, null, true, null));

        Assert.Contains("Prospecto", erro.Message);
    }
}
```

- [ ] **Step 2: Run test and confirm failure**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter ContratoTestes`

Expected: FAIL because `CriarDeOrcamento`, `ProspectId` and nullable `ClientId` are not implemented.

- [ ] **Step 3: Implement domain changes**

Modify `Contrato.cs`:

```csharp
public Guid? ClientId { get; private set; }
public Guid ProspectId { get; private set; }
public Guid? BudgetId { get; private set; }

public Prospecto Prospecto { get; private set; } = default!;
public Cliente? Cliente { get; private set; }
public Orcamento? Orcamento { get; private set; }

public static Contrato CriarDeOrcamento(
    Orcamento orcamento,
    Prospecto prospecto,
    DateTime dataInicio,
    DateTime? dataFim,
    bool renovacaoAutomatica,
    string? clausulasExtras)
{
    if (orcamento.ProspectId != prospecto.Id)
        throw new InvalidOperationException("Prospecto nao pertence ao orcamento informado.");

    return new Contrato
    {
        TenantId = orcamento.TenantId,
        ProspectId = prospecto.Id,
        ClientId = null,
        BudgetId = orcamento.Id,
        ValorTotal = orcamento.ValorTotal,
        DiaVencimento = orcamento.DiaVencimento,
        DataInicio = dataInicio,
        DataFim = dataFim,
        RenovacaoAutomatica = renovacaoAutomatica,
        ClausulasExtras = clausulasExtras,
        Status = StatusContrato.Rascunho,
        Numero = $"CTR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..4].ToUpper()}"
    };
}
```

Keep `CreateFromBudget(...)` only if existing PDF/assinatura code still needs it temporarily; if retained, adapt it to set `ProspectId` from `budget.ProspectId` and `ClientId` from the parameter.

- [ ] **Step 4: Run domain tests**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter ContratoTestes`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Officio.Dominio/Entidades/Contrato.cs Officio.Testes/Unitarios/Dominio/ContratoTestes.cs
git commit -m "AK: allow contrato from prospect orcamento"
```

---

### Task 2: Persistencia e Consultas EF de Contrato

**Files:**
- Modify: `Officio.Dominio/Interfaces/IContratoRepositorio.cs`
- Modify: `Officio.Infra.BancoDeDados/Repositorios/ContratoRepositorio.cs`
- Modify: `Officio.Infra.BancoDeDados/Mapeamentos/ContratoMap.cs`
- Test: `Officio.Testes/Unitarios/InfraBancoDeDados/ContratoRepositorioTestes.cs`

**Interfaces:**
- Consumes: `Contrato.CriarDeOrcamento(...)`.
- Produces: `AdicionarAsync`, `ObterPorOrcamentoAsync`, `ListarAsync`, `ObterDetalheAsync`.

- [ ] **Step 1: Write failing repository tests**

Create `Officio.Testes/Unitarios/InfraBancoDeDados/ContratoRepositorioTestes.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class ContratoRepositorioTestes
{
    [Fact]
    public async Task ObterPorOrcamentoAsync_RetornaContratoDoOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (prospecto, orcamento, contrato) = CriarContratoAceito(tenantId);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        var repositorio = new ContratoRepositorio(contexto);

        var resultado = await repositorio.ObterPorOrcamentoAsync(orcamento.Id);

        Assert.NotNull(resultado);
        Assert.Equal(contrato.Id, resultado.Id);
    }

    [Fact]
    public async Task ObterDetalheAsync_CarregaProspectoEOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (prospecto, orcamento, contrato) = CriarContratoAceito(tenantId);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var repositorio = new ContratoRepositorio(contexto);

        var resultado = await repositorio.ObterDetalheAsync(contrato.Id);

        Assert.NotNull(resultado);
        Assert.Equal(prospecto.RazaoSocial, resultado.Prospecto.RazaoSocial);
        Assert.Equal(orcamento.Id, resultado.Orcamento!.Id);
    }

    private static (Prospecto Prospecto, Orcamento Orcamento, Contrato Contrato) CriarContratoAceito(Guid tenantId)
    {
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios",
            SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 1, 500m, 1));
        orcamento.Aceitar();
        var contrato = Contrato.CriarDeOrcamento(orcamento, prospecto, new DateTime(2026, 9, 1), null, true, null);
        return (prospecto, orcamento, contrato);
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ContextoAplicacao(options, CriarEmpresaAtual(tenantId), new PublicadorEventosFake());
    }

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return empresaAtual;
    }
}
```

- [ ] **Step 2: Run repository test and confirm failure**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter ContratoRepositorioTestes`

Expected: FAIL because repository methods do not exist.

- [ ] **Step 3: Extend repository interface**

Modify `IContratoRepositorio.cs`:

```csharp
Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default);
Task<Contrato?> ObterPorOrcamentoAsync(Guid orcamentoId, CancellationToken cancellationToken = default);
Task<List<Contrato>> ListarAsync(CancellationToken cancellationToken = default);
Task<Contrato?> ObterDetalheAsync(Guid contratoId, CancellationToken cancellationToken = default);
```

- [ ] **Step 4: Implement EF repository methods**

Modify `ContratoRepositorio.cs`:

```csharp
public Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default)
    => AdicionarAsync<Contrato>(contrato, cancellationToken);

public Task<Contrato?> ObterPorOrcamentoAsync(Guid orcamentoId, CancellationToken cancellationToken = default)
    => Contexto.Contracts
        .FirstOrDefaultAsync(contrato => contrato.BudgetId == orcamentoId, cancellationToken);

public Task<List<Contrato>> ListarAsync(CancellationToken cancellationToken = default)
    => Contexto.Contracts
        .Include(contrato => contrato.Prospecto)
        .OrderByDescending(contrato => contrato.CriadoEm)
        .ToListAsync(cancellationToken);

public Task<Contrato?> ObterDetalheAsync(Guid contratoId, CancellationToken cancellationToken = default)
    => Contexto.Contracts
        .Include(contrato => contrato.Prospecto)
        .Include(contrato => contrato.Cliente)
        .Include(contrato => contrato.Orcamento)
            .ThenInclude(orcamento => orcamento!.Itens)
        .FirstOrDefaultAsync(contrato => contrato.Id == contratoId, cancellationToken);
```

Update `ObterParaGeracaoPdfAsync` later, in Task 5, to support `Prospecto` for PDF.

- [ ] **Step 5: Update EF mapping**

Modify `ContratoMap.cs`:

```csharp
builder.Property(c => c.ClientId)
    .IsRequired(false);

builder.HasOne(c => c.Prospecto)
    .WithMany()
    .HasForeignKey(c => c.ProspectId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(c => c.Cliente)
    .WithMany(c => c.Contratos)
    .HasForeignKey(c => c.ClientId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired(false);

builder.HasOne(c => c.Orcamento)
    .WithOne()
    .HasForeignKey<Contrato>(c => c.BudgetId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasIndex(c => new { c.TenantId, c.BudgetId })
    .IsUnique()
    .HasFilter("[BudgetId] IS NOT NULL");
```

- [ ] **Step 6: Run repository tests**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter ContratoRepositorioTestes`

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add Officio.Dominio/Interfaces/IContratoRepositorio.cs Officio.Infra.BancoDeDados/Repositorios/ContratoRepositorio.cs Officio.Infra.BancoDeDados/Mapeamentos/ContratoMap.cs Officio.Testes/Unitarios/InfraBancoDeDados/ContratoRepositorioTestes.cs
git commit -m "AK: add contrato repository queries"
```

---

### Task 3: Servico de Conversao e Consulta de Contratos

**Files:**
- Modify: `Officio.Dominio/Contratos/ContratosContratos.cs`
- Modify: `Officio.Dominio/Interfaces/IContratoServico.cs`
- Modify: `Officio.Dominio.Servicos/Servicos/ContratoServico.cs`
- Test: `Officio.Testes/Unitarios/DominioServicos/ContratoServicoTestes.cs`

**Interfaces:**
- Consumes: `IContratoRepositorio`, `IOrcamentoRepositorio`, `IProspectoRepositorio`.
- Produces: `ConverterOrcamentoAsync`, `ListarAsync`, `ObterPorIdAsync`.

- [ ] **Step 1: Write failing service tests**

Extend `Officio.Testes/Unitarios/DominioServicos/ContratoServicoTestes.cs`:

```csharp
[Fact]
public async Task ConverterOrcamentoAsync_ComOrcamentoAceito_CriaContrato()
{
    var tenantId = Guid.NewGuid();
    await using var contexto = CriarContexto(tenantId);
    var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
    var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
    orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
        tenantId, orcamento.Id, null, "Honorarios",
        SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 1, 500m, 1));
    orcamento.Aceitar();
    contexto.Prospects.Add(prospecto);
    contexto.Budgets.Add(orcamento);
    await contexto.SaveChangesAsync();
    var servico = CriarServico(contexto, tenantId);

    var resultado = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
        orcamento.Id,
        new DateTime(2026, 9, 1),
        null,
        true,
        null));

    Assert.True(resultado.Sucesso);
    var contrato = await contexto.Contracts.SingleAsync();
    Assert.Equal(prospecto.Id, contrato.ProspectId);
    Assert.Null(contrato.ClientId);
    Assert.Equal(orcamento.Id, contrato.BudgetId);
}

[Fact]
public async Task ConverterOrcamentoAsync_ComOrcamentoRascunho_Falha()
{
    var tenantId = Guid.NewGuid();
    await using var contexto = CriarContexto(tenantId);
    var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
    var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
    contexto.Prospects.Add(prospecto);
    contexto.Budgets.Add(orcamento);
    await contexto.SaveChangesAsync();
    var servico = CriarServico(contexto, tenantId);

    var resultado = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
        orcamento.Id,
        DateTime.UtcNow,
        null,
        true,
        null));

    Assert.False(resultado.Sucesso);
    Assert.Contains("aceitos", resultado.Erro, StringComparison.OrdinalIgnoreCase);
}
```

Add helper in the test file:

```csharp
private static IContratoServico CriarServico(ContextoAplicacao contexto, Guid tenantId)
    => new ContratoServico(
        new ContratoRepositorio(contexto),
        new EmpresaRepositorio(contexto),
        new OrcamentoRepositorio(contexto),
        new ProspectoRepositorio(contexto),
        new ServicoPdfFake(),
        new ServicoAssinaturaFake(),
        new ServicoArmazenamentoFake(),
        CriarEmpresaAtual(tenantId));
```

If `ContratoServico` remains `internal`, either use an existing `InternalsVisibleTo` pattern or make the class public to keep behavior tests direct.

- [ ] **Step 2: Run service tests and confirm failure**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter ContratoServicoTestes`

Expected: FAIL because contracts/methods are missing.

- [ ] **Step 3: Add domain contracts**

Modify `ContratosContratos.cs`:

```csharp
public record ConverterOrcamentoContratoRequisicao(
    Guid OrcamentoId,
    DateTime DataInicio,
    DateTime? DataFim,
    bool RenovacaoAutomatica,
    string? ClausulasExtras);

public record ContratoResultado(
    Guid Id,
    string Numero,
    Guid ProspectoId,
    Guid? ClienteId,
    Guid? OrcamentoId,
    string RazaoSocial,
    string Status,
    decimal ValorTotal,
    int DiaVencimento,
    DateTime DataInicio,
    DateTime? DataFim,
    bool RenovacaoAutomatica,
    string? ClausulasExtras,
    bool PdfGerado);
```

- [ ] **Step 4: Extend service interface**

Modify `IContratoServico.cs`:

```csharp
Task<ResultadoOperacao<Guid>> ConverterOrcamentoAsync(
    ConverterOrcamentoContratoRequisicao requisicao,
    CancellationToken ct = default);

Task<ResultadoOperacao<List<ContratoResultado>>> ListarAsync(CancellationToken ct = default);

Task<ResultadoOperacao<ContratoResultado>> ObterPorIdAsync(Guid contratoId, CancellationToken ct = default);
```

- [ ] **Step 5: Implement service logic**

Modify constructor of `ContratoServico` to also receive:

```csharp
IOrcamentoRepositorio orcamentoRepositorio,
IProspectoRepositorio prospectoRepositorio,
```

Implement:

```csharp
public async Task<ResultadoOperacao<Guid>> ConverterOrcamentoAsync(
    ConverterOrcamentoContratoRequisicao requisicao,
    CancellationToken ct = default)
{
    var orcamento = await orcamentoRepositorio.ObterPorIdComItensAsync(requisicao.OrcamentoId, ct);
    if (orcamento is null)
        return ResultadoOperacao<Guid>.CriarFalha("Orcamento nao encontrado.");

    if (orcamento.Status != StatusOrcamento.Aceito)
        return ResultadoOperacao<Guid>.CriarFalha("Somente orcamentos aceitos podem ser convertidos em contrato.");

    if (await contratoRepositorio.ObterPorOrcamentoAsync(orcamento.Id, ct) is not null)
        return ResultadoOperacao<Guid>.CriarFalha("Ja existe contrato para este orcamento.");

    var prospecto = await prospectoRepositorio.ObterComSociosAsync(orcamento.ProspectId, ct);
    if (prospecto is null)
        return ResultadoOperacao<Guid>.CriarFalha("Prospecto nao encontrado.");

    var contrato = Contrato.CriarDeOrcamento(
        orcamento,
        prospecto,
        requisicao.DataInicio,
        requisicao.DataFim,
        requisicao.RenovacaoAutomatica,
        requisicao.ClausulasExtras);

    await contratoRepositorio.AdicionarAsync(contrato, ct);
    await contratoRepositorio.SalvarAlteracoesAsync(ct);

    return ResultadoOperacao<Guid>.CriarSucesso(contrato.Id);
}
```

Implement `ListarAsync` and `ObterPorIdAsync` using a private mapper:

```csharp
private static ContratoResultado MapearContrato(Contrato contrato)
{
    var razaoSocial = contrato.Cliente?.RazaoSocial ?? contrato.Prospecto.RazaoSocial;

    return new ContratoResultado(
        contrato.Id,
        contrato.Numero,
        contrato.ProspectId,
        contrato.ClientId,
        contrato.BudgetId,
        razaoSocial,
        contrato.Status.ToString(),
        contrato.ValorTotal,
        contrato.DiaVencimento,
        contrato.DataInicio,
        contrato.DataFim,
        contrato.RenovacaoAutomatica,
        contrato.ClausulasExtras,
        !string.IsNullOrWhiteSpace(contrato.PdfPath));
}
```

- [ ] **Step 6: Run service tests**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter ContratoServicoTestes`

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add Officio.Dominio/Contratos/ContratosContratos.cs Officio.Dominio/Interfaces/IContratoServico.cs Officio.Dominio.Servicos/Servicos/ContratoServico.cs Officio.Testes/Unitarios/DominioServicos/ContratoServicoTestes.cs
git commit -m "AK: add contrato conversion service"
```

---

### Task 4: MediatR Commands, Queries e Handlers Finos

**Files:**
- Create: `Officio.Dominio.Mediador/Funcionalidades/Orcamentos/Comandos/ConverterParaContrato/ConverterOrcamentoParaContratoComando.cs`
- Create: `Officio.Dominio.Mediador/Funcionalidades/Orcamentos/Comandos/ConverterParaContrato/ConverterOrcamentoParaContratoHandler.cs`
- Create: `Officio.Dominio.Mediador/Funcionalidades/Contratos/Consultas/ListarContratos/ListarContratosConsulta.cs`
- Create: `Officio.Dominio.Mediador/Funcionalidades/Contratos/Consultas/ListarContratos/ListarContratosHandler.cs`
- Create: `Officio.Dominio.Mediador/Funcionalidades/Contratos/Consultas/ObterContratoPorId/ObterContratoPorIdConsulta.cs`
- Create: `Officio.Dominio.Mediador/Funcionalidades/Contratos/Consultas/ObterContratoPorId/ObterContratoPorIdHandler.cs`
- Create: `Officio.Dominio.Mediador/Funcionalidades/Contratos/Consultas/ContratoDto.cs`
- Test: `Officio.Testes/Unitarios/DominioMediador/ContratoHandlersArquiteturaTestes.cs`
- Test: `Officio.Testes/Unitarios/DominioMediador/ContratoHandlersTestes.cs`

**Interfaces:**
- Consumes: `IContratoServico`.
- Produces: handlers depending only on `IContratoServico`.

- [ ] **Step 1: Write architecture test**

Create `ContratoHandlersArquiteturaTestes.cs`:

```csharp
using System.Reflection;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;

namespace Officio.Testes.Unitarios.DominioMediador;

public class ContratoHandlersArquiteturaTestes
{
    public static TheoryData<Type> HandlersContrato => new()
    {
        typeof(ConverterOrcamentoParaContratoHandler),
        typeof(ListarContratosHandler),
        typeof(ObterContratoPorIdHandler)
    };

    [Theory]
    [MemberData(nameof(HandlersContrato))]
    public void HandlerDeContrato_DeveDependerSomenteDoServicoDeContrato(Type handlerType)
    {
        var construtor = Assert.Single(handlerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var dependencias = construtor.GetParameters().Select(parametro => parametro.ParameterType).ToArray();

        Assert.Equal([typeof(IContratoServico)], dependencias);
    }
}
```

- [ ] **Step 2: Write handler behavior tests**

Create `ContratoHandlersTestes.cs` with a fake service:

```csharp
[Fact]
public async Task ConverterOrcamentoParaContrato_ChamaServicoERetornaId()
{
    var contratoId = Guid.NewGuid();
    var servico = new ContratoServicoFake(ResultadoOperacao<Guid>.CriarSucesso(contratoId));
    var handler = new ConverterOrcamentoParaContratoHandler(servico);

    var resultado = await handler.Handle(new ConverterOrcamentoParaContratoComando(
        Guid.NewGuid(),
        new DateTime(2026, 9, 1),
        null,
        true,
        "Clausula"), CancellationToken.None);

    Assert.True(resultado.Sucesso);
    Assert.Equal(contratoId, resultado.Valor);
}
```

The fake should implement only the methods used by the tests and throw `NotImplementedException` for others.

- [ ] **Step 3: Run handler tests and confirm failure**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter "ContratoHandlersArquiteturaTestes|ContratoHandlersTestes"`

Expected: FAIL because handlers and commands do not exist.

- [ ] **Step 4: Implement command/query records and DTO**

Create:

```csharp
public record ConverterOrcamentoParaContratoComando(
    Guid OrcamentoId,
    DateTime DataInicio,
    DateTime? DataFim,
    bool RenovacaoAutomatica,
    string? ClausulasExtras) : IRequest<Resultado<Guid>>;
```

```csharp
public record ListarContratosConsulta : IRequest<Resultado<List<ContratoDto>>>;
public record ObterContratoPorIdConsulta(Guid ContratoId) : IRequest<Resultado<ContratoDto>>;
```

```csharp
public record ContratoDto(
    Guid Id,
    string Numero,
    Guid ProspectoId,
    Guid? ClienteId,
    Guid? OrcamentoId,
    string RazaoSocial,
    string Status,
    decimal ValorTotal,
    int DiaVencimento,
    DateTime DataInicio,
    DateTime? DataFim,
    bool RenovacaoAutomatica,
    string? ClausulasExtras,
    bool PdfGerado);
```

- [ ] **Step 5: Implement thin handlers**

Handlers must call `IContratoServico` and use `ResultadoOperacaoConversor`.

Example:

```csharp
public class ConverterOrcamentoParaContratoHandler(IContratoServico contratoServico)
    : IRequestHandler<ConverterOrcamentoParaContratoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(ConverterOrcamentoParaContratoComando request, CancellationToken ct)
    {
        var requisicao = new ConverterOrcamentoContratoRequisicao(
            request.OrcamentoId,
            request.DataInicio,
            request.DataFim,
            request.RenovacaoAutomatica,
            request.ClausulasExtras);

        return (await contratoServico.ConverterOrcamentoAsync(requisicao, ct)).ParaResultado();
    }
}
```

For queries, map `ContratoResultado` to `ContratoDto` in the handler.

- [ ] **Step 6: Run handler tests**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter "ContratoHandlersArquiteturaTestes|ContratoHandlersTestes"`

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add Officio.Dominio.Mediador/Funcionalidades/Orcamentos/Comandos/ConverterParaContrato Officio.Dominio.Mediador/Funcionalidades/Contratos/Consultas Officio.Testes/Unitarios/DominioMediador/ContratoHandlersArquiteturaTestes.cs Officio.Testes/Unitarios/DominioMediador/ContratoHandlersTestes.cs
git commit -m "AK: add contrato mediator flow"
```

---

### Task 5: API ViewModel, Conversor e Endpoints

**Files:**
- Create: `Officio.ViewModels/Contratos/ConverterOrcamentoParaContratoViewModel.cs`
- Modify: `Officio.WebApi/Conversores/ContratoViewModelConversor.cs`
- Modify: `Officio.WebApi/Controllers/OrcamentosControlador.cs`
- Modify: `Officio.WebApi/Controllers/ContratosControlador.cs`
- Test: `Officio.Testes/Unitarios/WebApi/ContratoViewModelConversorTestes.cs`
- Test: `Officio.Testes/Unitarios/WebApi/ContratoControllerRotasTestes.cs`

**Interfaces:**
- Consumes: MediatR commands/queries from Task 4.
- Produces: API endpoints with `[action]` route style.

- [ ] **Step 1: Write converter tests**

Create or extend `ContratoViewModelConversorTestes.cs`:

```csharp
[Fact]
public void ConverterOrcamentoParaContratoViewModel_ParaComando_MapeiaCampos()
{
    var orcamentoId = Guid.NewGuid();
    var viewModel = new ConverterOrcamentoParaContratoViewModel
    {
        DataInicio = new DateTime(2026, 9, 1),
        DataFim = new DateTime(2027, 9, 1),
        RenovacaoAutomatica = true,
        ClausulasExtras = "Clausula"
    };

    var comando = viewModel.ParaComando(orcamentoId);

    Assert.Equal(orcamentoId, comando.OrcamentoId);
    Assert.Equal(viewModel.DataInicio, comando.DataInicio);
    Assert.Equal(viewModel.DataFim, comando.DataFim);
    Assert.True(comando.RenovacaoAutomatica);
    Assert.Equal("Clausula", comando.ClausulasExtras);
}
```

- [ ] **Step 2: Write route tests**

Add route tests matching existing controller route test style:

```csharp
[Fact]
public void OrcamentosControlador_ConverterParaContrato_UsaRotaActionComId()
{
    var metodo = typeof(OrcamentosControlador).GetMethod(nameof(OrcamentosControlador.ConverterParaContrato));

    var atributo = Assert.Single(metodo!.GetCustomAttributes<HttpPostAttribute>());

    Assert.Equal("{id:guid}", atributo.Template);
}
```

Add similar tests for `ContratosControlador.Listar` and `ContratosControlador.ObterPorId`.

- [ ] **Step 3: Run API tests and confirm failure**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter "ContratoViewModelConversorTestes|ContratoControllerRotasTestes"`

Expected: FAIL because ViewModel/converter/endpoints do not exist.

- [ ] **Step 4: Implement ViewModel and converter**

Create:

```csharp
namespace Officio.ViewModels.Contratos;

public class ConverterOrcamentoParaContratoViewModel
{
    public DateTime DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public bool RenovacaoAutomatica { get; init; }
    public string? ClausulasExtras { get; init; }
}
```

Extend `ContratoViewModelConversor.cs`:

```csharp
public static ConverterOrcamentoParaContratoComando ParaComando(
    this ConverterOrcamentoParaContratoViewModel viewModel,
    Guid orcamentoId)
    => new(
        orcamentoId,
        viewModel.DataInicio,
        viewModel.DataFim,
        viewModel.RenovacaoAutomatica,
        viewModel.ClausulasExtras);
```

- [ ] **Step 5: Implement controller endpoints**

In `OrcamentosControlador`:

```csharp
[HttpPost("{id:guid}")]
public async Task<IActionResult> ConverterParaContrato(
    Guid id,
    [FromBody] ConverterOrcamentoParaContratoViewModel viewModel,
    CancellationToken cancellationToken)
{
    var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);

    return resultado.Sucesso
        ? CreatedAtAction("ObterPorId", "Contratos", new { id = resultado.Valor }, resultado.Valor)
        : ResultadoErro(resultado.Erro);
}
```

In `ContratosControlador`, add private `ResultadoErro` helper equivalent to `OrcamentosControlador` and:

```csharp
[HttpGet]
public async Task<IActionResult> Listar(CancellationToken cancellationToken)
{
    var resultado = await mediator.Send(new ListarContratosConsulta(), cancellationToken);
    return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
}

[HttpGet("{id:guid}")]
public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
{
    var resultado = await mediator.Send(new ObterContratoPorIdConsulta(id), cancellationToken);
    return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
}
```

- [ ] **Step 6: Run API tests**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter "ContratoViewModelConversorTestes|ContratoControllerRotasTestes"`

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add Officio.ViewModels/Contratos/ConverterOrcamentoParaContratoViewModel.cs Officio.WebApi/Conversores/ContratoViewModelConversor.cs Officio.WebApi/Controllers/OrcamentosControlador.cs Officio.WebApi/Controllers/ContratosControlador.cs Officio.Testes/Unitarios/WebApi/ContratoViewModelConversorTestes.cs Officio.Testes/Unitarios/WebApi/ContratoControllerRotasTestes.cs
git commit -m "AK: expose contrato conversion api"
```

---

### Task 6: PDF Compatibility With Prospect Contracts

**Files:**
- Modify: `Officio.Dominio.Servicos/Servicos/ContratoServico.cs`
- Modify: `Officio.Infra.BancoDeDados/Repositorios/ContratoRepositorio.cs`
- Test: `Officio.Testes/Unitarios/DominioServicos/ContratoServicoTestes.cs`

**Interfaces:**
- Consumes: `Contrato.Prospecto`, `Contrato.Cliente?`.
- Produces: PDF generation that works for pre-assinatura contracts using prospect data.

- [ ] **Step 1: Write failing PDF mapping test**

Extend `ContratoServicoTestes.cs` with a fake PDF service that captures `DadosPdfContrato`:

```csharp
[Fact]
public async Task GerarPdfAsync_ComContratoVinculadoAoProspecto_UsaDadosDoProspecto()
{
    var tenantId = Guid.NewGuid();
    await using var contexto = CriarContexto(tenantId);
    var empresa = Empresa.Create("Contabilidade Teste", "00999888000177");
    var prospecto = Prospecto.Create(tenantId, "Empresa Prospect", "11222333000144", null);
    var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
    orcamento.Aceitar();
    var contrato = Contrato.CriarDeOrcamento(orcamento, prospecto, new DateTime(2026, 9, 1), null, true, null);
    contexto.Empresas.Add(empresa);
    contexto.Prospects.Add(prospecto);
    contexto.Budgets.Add(orcamento);
    contexto.Contracts.Add(contrato);
    await contexto.SaveChangesAsync();
    var pdf = new ServicoPdfCapturaFake();
    var servico = CriarServico(contexto, tenantId, pdf);

    var resultado = await servico.GerarPdfAsync(contrato.Id);

    Assert.True(resultado.Sucesso);
    Assert.Equal("Empresa Prospect", pdf.UltimosDados!.Contratante.RazaoSocial);
}
```

- [ ] **Step 2: Run PDF test and confirm failure**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter GerarPdfAsync_ComContratoVinculadoAoProspecto_UsaDadosDoProspecto`

Expected: FAIL because current PDF mapping assumes `Cliente` is required.

- [ ] **Step 3: Include prospect in PDF repository query**

Modify `ObterParaGeracaoPdfAsync`:

```csharp
=> await Contexto.Contracts
    .Include(c => c.Prospecto)
        .ThenInclude(p => p.Socios)
    .Include(c => c.Cliente)
        .ThenInclude(cl => cl!.Socios)
    .Include(c => c.Orcamento)
        .ThenInclude(o => o!.Itens)
    .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);
```

- [ ] **Step 4: Map PDF from cliente or prospect**

In `MapearDadosPdf`, replace direct `contrato.Cliente` usage with fallback:

```csharp
var razaoSocial = contrato.Cliente?.RazaoSocial ?? contrato.Prospecto.RazaoSocial;
var nomeFantasia = contrato.Cliente?.NomeFantasia ?? contrato.Prospecto.NomeFantasia ?? razaoSocial;
var cnpj = contrato.Cliente?.Cnpj ?? contrato.Prospecto.Cnpj ?? "";
var email = contrato.Cliente?.Email ?? contrato.Prospecto.Email ?? "";
var telefone = contrato.Cliente?.Telefone ?? contrato.Prospecto.Telefone ?? "";
var socios = contrato.Cliente?.Socios ?? contrato.Prospecto.Socios;
```

Then create `DadosContratante` from these values. Preserve current client path when `Cliente` exists.

- [ ] **Step 5: Run PDF test**

Run: `dotnet test Officio.Testes/Officio.Testes.csproj --filter GerarPdfAsync_ComContratoVinculadoAoProspecto_UsaDadosDoProspecto`

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add Officio.Dominio.Servicos/Servicos/ContratoServico.cs Officio.Infra.BancoDeDados/Repositorios/ContratoRepositorio.cs Officio.Testes/Unitarios/DominioServicos/ContratoServicoTestes.cs
git commit -m "AK: support prospect contrato pdf data"
```

---

### Task 7: Migration e Verificacao Final

**Files:**
- Create: `Officio.Infra.BancoDeDados/Migrations/<timestamp>_AdicionarContratoProspecto.cs`
- Modify: `Officio.Infra.BancoDeDados/Migrations/ContextoAplicacaoModelSnapshot.cs`

**Interfaces:**
- Consumes: EF mapping from Tasks 1-2.
- Produces: SQL Server migration for pre-assinatura contracts.

- [ ] **Step 1: Generate migration**

Run:

```bash
dotnet ef migrations add AdicionarContratoProspecto --project Officio.Infra.BancoDeDados --startup-project Officio.Infra.BancoDeDados --output-dir Migrations
```

Expected: migration adds `ProspectId` to `Contracts`, makes `ClientId` nullable, creates FK to `Prospects`, and adds unique index for `{ TenantId, BudgetId }`.

- [ ] **Step 2: Review migration**

Check generated migration:

```bash
git diff -- Officio.Infra.BancoDeDados/Migrations
```

Expected:
- no unrelated `DropTable`;
- no destructive unrelated column changes;
- `ClientId` changed to nullable;
- `ProspectId` has FK to `Prospects`;
- unique index for budget conversion exists.

- [ ] **Step 3: Run full tests**

Run:

```bash
dotnet test Officio.Testes/Officio.Testes.csproj
```

Expected: PASS.

- [ ] **Step 4: Run build**

Run:

```bash
dotnet build Officio.slnx
```

Expected: PASS with 0 warnings and 0 errors.

- [ ] **Step 5: Search forbidden patterns**

Run:

```bash
rg "I[A-Za-z]+Repositorio|IEmpresaAtual" Officio.Dominio.Mediador -g "*Handler.cs" -g "*Manipulador.cs"
rg "ViewModel.*mediator.Send|Where\\(.*TenantId|public set" Officio.WebApi Officio.Dominio.Mediador Officio.Dominio Officio.Infra.BancoDeDados
```

Expected:
- first command has no matches;
- second command has no new violations from this block.

- [ ] **Step 6: Commit migration**

```bash
git add Officio.Infra.BancoDeDados/Migrations
git commit -m "AK: add contrato prospecto migration"
```

---

## Final Review Checklist

- [ ] Contract creation does not create `Cliente`.
- [ ] `Contrato` stores `ProspectId` and nullable `ClientId`.
- [ ] Duplicate conversion from same orcamento fails.
- [ ] Only accepted budgets can be converted.
- [ ] Contract queries return prospect data before assinatura.
- [ ] PDF generation does not require `Cliente` for pre-assinatura contracts.
- [ ] Handlers depend only on `IContratoServico`.
- [ ] Controllers convert ViewModels to Commands before MediatR.
- [ ] No D4Sign/webhook/fallback/client activation logic added.
- [ ] All commits use `AK:` prefix.
- [ ] `dotnet test Officio.Testes/Officio.Testes.csproj` passes.
- [ ] `dotnet build Officio.slnx` passes.

## Self-Review

- Spec coverage: conversion, contract queries, prospect-before-client decision, API, persistence, service boundary, tests and out-of-scope items are covered.
- Placeholder scan: no incomplete marker, vague error step, or undefined feature remains.
- Type consistency: method names and DTO names match the spec and the existing repository patterns.
