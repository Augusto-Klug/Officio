using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class OrcamentoVersao : EntidadeAuditavel
{
    private readonly List<OrcamentoItem> _itens = [];

    public Guid OrcamentoId { get; private set; }
    public Guid StatusOrcamentoId { get; private set; }
    public int NumeroVersao { get; private set; }
    public Guid? DocumentoId { get; private set; }
    public string? TipoEmpresaSnapshot { get; private set; }
    public string? RegimeTributarioSnapshot { get; private set; }
    public int QuantidadeFuncionariosSnapshot { get; private set; }
    public int QuantidadeProLaboresSnapshot { get; private set; }
    public int DiaVencimento { get; private set; }
    public DateTime? ValidoAte { get; private set; }
    public DateTime? EnviadoEm { get; private set; }
    public DateTime? AceitoEm { get; private set; }
    public string? MotivoRecusa { get; private set; }
    public string? Observacoes { get; private set; }
    public decimal ValorTotal { get; private set; }
    public Orcamento Orcamento { get; private set; } = default!;
    public StatusOrcamentoDominio StatusOrcamento { get; private set; } = default!;
    public Documento? Documento { get; private set; }
    public IReadOnlyCollection<OrcamentoItem> Itens => _itens.AsReadOnly();

    private OrcamentoVersao() { }

    public static OrcamentoVersao Criar(
        Guid tenantId,
        Guid orcamentoId,
        Guid statusOrcamentoId,
        int numeroVersao,
        int diaVencimento)
        => new()
        {
            TenantId = tenantId,
            OrcamentoId = orcamentoId,
            StatusOrcamentoId = statusOrcamentoId,
            NumeroVersao = numeroVersao,
            DiaVencimento = diaVencimento
        };

    public void AdicionarItem(OrcamentoItem item)
    {
        _itens.Add(item);
        ValorTotal = _itens.Sum(i => i.ValorTotal);
    }
}
