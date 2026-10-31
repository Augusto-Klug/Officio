using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Contrato : EntidadeAuditavel
{
    public string Numero { get; private set; } = default!;
    public Guid EmpresaId { get; private set; }
    public Guid? OrcamentoVersaoAceitaId { get; private set; }
    public Guid? ModeloContratoId { get; private set; }
    public Guid? StatusContratoId { get; private set; }
    public Guid? ClientId { get; private set; }
    public Guid ProspectId { get; private set; }
    public Guid? BudgetId { get; private set; }
    public Guid? RepresentanteSocioId { get; private set; }
    public Socio? RepresentanteSocio { get; private set; }
    public StatusContrato Status { get; private set; } = StatusContrato.Rascunho;
    public decimal ValorTotal { get; private set; }
    public int DiaVencimento { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
    public int? DuracaoAnos { get; private set; }
    public DateOnly? CompetenciaInicialHonorarios { get; private set; }
    public string? ClausulasExtras { get; private set; }
    public string? PdfPath { get; private set; }
    public byte[]? VersaoConcorrencia { get; private set; }
    public DateTime? AssinadoEm { get; private set; }

    // Navigation
    public Prospecto Prospecto { get; private set; } = default!;
    public Empresa? Empresa { get; private set; }
    public Cliente? Cliente { get; private set; }
    public Orcamento? Orcamento { get; private set; }
    public OrcamentoVersao? OrcamentoVersaoAceita { get; private set; }
    public ModeloContrato? ModeloContrato { get; private set; }
    public StatusContratoDominio? StatusContratoDominio { get; private set; }
    public ICollection<Assinatura> Assinaturas { get; private set; } = [];
    public ICollection<ContratoVersao> Versoes { get; private set; } = [];
    public ICollection<AssinaturaSolicitacao> SolicitacoesAssinatura { get; private set; } = [];
    public ICollection<FaturamentoMensal> FaturamentosMensais { get; private set; } = [];

    public decimal? ValorHonorarioMensal => Orcamento?.Itens
        .FirstOrDefault(item => item.NaturezaCobranca == NaturezaCobrancaItemOrcamento.HonorarioMensal)?.ValorUnitario;
    public decimal? ValorUnitarioPorFolha => Orcamento?.Itens
        .FirstOrDefault(item => item.NaturezaCobranca == NaturezaCobrancaItemOrcamento.PorFolha)?.ValorUnitario;
    public int DiaVencimentoComercial => Orcamento?.DiaVencimento ?? DiaVencimento;

    private Contrato() { }

    // Fábricas internas preservadas para cenários legados; novos contratos usam CriarComRepresentante.
    internal static Contrato CreateFromBudget(Orcamento budget, Guid clientId, DateTime dataInicio, DateTime? dataFim, string? clausulasExtras)
    {
        return new Contrato
        {
            TenantId = budget.TenantId,
            ProspectId = budget.ProspectId,
            ClientId = clientId,
            BudgetId = budget.Id,
            ValorTotal = budget.ValorTotal,
            DiaVencimento = budget.DiaVencimento,
            DataInicio = dataInicio,
            DataFim = dataFim,
            ClausulasExtras = clausulasExtras,
            Numero = $"CTR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..4].ToUpper()}"
        };
    }

    internal static Contrato CriarDeOrcamento(
        Orcamento orcamento,
        Prospecto prospecto,
        DateTime dataInicio,
        int duracaoAnos,
        DateOnly? competenciaInicialHonorarios,
        string? clausulasExtras)
    {
        if (orcamento.ProspectId != prospecto.Id)
            throw new InvalidOperationException("Prospecto nao pertence ao orcamento informado.");
        ValidarDuracao(duracaoAnos);

        var competencia = competenciaInicialHonorarios ?? new DateOnly(dataInicio.Year, dataInicio.Month, 1);
        return new Contrato
        {
            TenantId = orcamento.TenantId,
            ProspectId = prospecto.Id,
            EmpresaId = prospecto.EmpresaId.GetValueOrDefault(),
            ClientId = null,
            BudgetId = orcamento.Id,
            ValorTotal = orcamento.ValorTotal,
            DiaVencimento = orcamento.DiaVencimento,
            DataInicio = dataInicio,
            DataFim = dataInicio.AddYears(duracaoAnos),
            DuracaoAnos = duracaoAnos,
            CompetenciaInicialHonorarios = new DateOnly(competencia.Year, competencia.Month, 1),
            ClausulasExtras = clausulasExtras,
            Status = StatusContrato.Rascunho,
            Numero = $"CTR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..4].ToUpper()}"
        };
    }

    internal static Contrato CriarDeOrcamento(
        Orcamento orcamento,
        Prospecto prospecto,
        DateTime dataInicio,
        DateTime? dataFim,
        string? clausulasExtras)
    {
        if (orcamento.ProspectId != prospecto.Id)
            throw new InvalidOperationException("Prospecto nao pertence ao orcamento informado.");

        return new Contrato
        {
            TenantId = orcamento.TenantId,
            ProspectId = prospecto.Id,
            EmpresaId = prospecto.EmpresaId.GetValueOrDefault(),
            ClientId = null,
            BudgetId = orcamento.Id,
            ValorTotal = orcamento.ValorTotal,
            DiaVencimento = orcamento.DiaVencimento,
            DataInicio = dataInicio,
            DataFim = dataFim,
            ClausulasExtras = clausulasExtras,
            Status = StatusContrato.Rascunho,
            Numero = $"CTR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..4].ToUpper()}"
        };
    }

    public void SetPdfPath(string pdfPath) => PdfPath = pdfPath;

    public static Contrato CriarComRepresentante(Orcamento orcamento, Prospecto prospecto, Socio representante,
        Guid? clienteCorrespondenteId, DateTime dataInicio, int duracaoAnos,
        DateOnly? competenciaInicialHonorarios, string? clausulasExtras)
    {
        if (orcamento.TenantId != prospecto.TenantId || representante.TenantId != prospecto.TenantId
            || (representante.ProspectId != prospecto.Id &&
                (!clienteCorrespondenteId.HasValue || representante.ClientId != clienteCorrespondenteId)))
            throw new InvalidOperationException("Representante não pertence ao cadastro do contrato.");
        if (CadastroRepresentante.InformacoesFaltantes(representante).Count > 0)
            throw new InvalidOperationException("Complete o cadastro do representante antes de criar o contrato.");
        var contrato = CriarDeOrcamento(
            orcamento,
            prospecto,
            dataInicio,
            duracaoAnos,
            competenciaInicialHonorarios,
            clausulasExtras);
        contrato.RepresentanteSocio = representante;
        contrato.RepresentanteSocioId = representante.Id;
        return contrato;
    }

    private static void ValidarDuracao(int duracaoAnos)
    {
        if (duracaoAnos is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(duracaoAnos), "A duracao do contrato deve estar entre 1 e 100 anos.");
    }

    public void MarcarAguardandoAssinatura()
    {
        if (Status != StatusContrato.Rascunho)
            throw new InvalidOperationException("Somente contratos em rascunho podem aguardar assinatura.");
        Status = StatusContrato.AguardandoAssinatura;
    }

    public void Encerrar()
    {
        if (Status == StatusContrato.Ativo)
            Status = StatusContrato.Encerrado;
    }

    public void Cancelar()
    {
        if (Status is not (StatusContrato.Rascunho or StatusContrato.AguardandoAssinatura))
            throw new InvalidOperationException("Somente contratos em rascunho ou aguardando assinatura podem ser cancelados.");
        Status = StatusContrato.Cancelado;
    }

    public void VincularCliente(Guid clientId)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("Cliente e obrigatorio.", nameof(clientId));

        if (ClientId.HasValue && ClientId != clientId)
            throw new InvalidOperationException("Contrato ja esta vinculado a outro cliente.");

        ClientId = clientId;
    }

    public void VincularEmpresa(Guid empresaId)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("Empresa e obrigatoria.", nameof(empresaId));

        if (EmpresaId != Guid.Empty && EmpresaId != empresaId)
            throw new InvalidOperationException("Contrato ja esta vinculado a outra empresa.");

        EmpresaId = empresaId;
    }

    public void MarkAsSigned(DateTime? assinadoEm = null)
    {
        if (Status == StatusContrato.Cancelado || (AssinadoEm.HasValue && Status == StatusContrato.Encerrado))
            return;

        var jaEstavaAssinado = AssinadoEm.HasValue;
        Status = StatusContrato.Ativo;
        AssinadoEm ??= assinadoEm ?? DateTime.UtcNow;

        if (!jaEstavaAssinado)
            AddDomainEvent(new EventoContratoAssinado(Id, ClientId, TenantId));
    }
}





