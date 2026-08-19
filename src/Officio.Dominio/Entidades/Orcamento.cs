using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Orcamento : EntidadeAuditavel
{
    private List<ItemOrcamento> _itens = [];

    public Guid ProspectId { get; private set; }
    public string? Titulo { get; private set; }
    public decimal ValorMensalidade { get; private set; }
    public decimal ValorPorFuncionario { get; private set; }
    public int QtdFuncionarios { get; private set; }
    public decimal ValorTaxasExtras { get; private set; }
    public decimal Desconto { get; private set; }
    public int DiaVencimento { get; private set; }
    public string? Observacoes { get; private set; }
    public decimal ValorTotal { get; private set; }
    public StatusOrcamento Status { get; private set; } = StatusOrcamento.Rascunho;
    public string? MotivoRecusa { get; private set; }
    public Prospecto Prospecto { get; private set; } = default!;
    public IReadOnlyCollection<ItemOrcamento> Itens => _itens.AsReadOnly();

    private Orcamento() { }

    public static Orcamento Criar(
        Guid tenantId,
        Guid prospectId,
        string? titulo,
        int diaVencimento,
        string? observacoes)
    {
        return new Orcamento
        {
            TenantId = tenantId,
            ProspectId = prospectId,
            Titulo = titulo,
            DiaVencimento = diaVencimento,
            Observacoes = observacoes,
            Status = StatusOrcamento.Rascunho
        };
    }

    public void Atualizar(string? titulo, int diaVencimento, string? observacoes)
    {
        ValidarEdicaoPermitida();

        Titulo = titulo;
        DiaVencimento = diaVencimento;
        Observacoes = observacoes;
    }

    public void Aceitar()
    {
        if (Status == StatusOrcamento.Aceito)
            throw new InvalidOperationException("Orcamento ja esta aceito.");

        Status = StatusOrcamento.Aceito;
    }

    public void Recusar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo da recusa e obrigatorio.", nameof(motivo));

        Status = StatusOrcamento.Recusado;
        MotivoRecusa = motivo;
    }

    public void AlterarStatus(StatusOrcamento status) => Status = status;

    public void AdicionarItem(ItemOrcamento item)
    {
        ValidarEdicaoPermitida();
        _itens.Add(item);
        ValorTotal = _itens.Sum(i => i.ValorTotal);
    }

    public void SubstituirItens(IEnumerable<ItemOrcamento> itens)
    {
        ValidarEdicaoPermitida();
        _itens.Clear();
        _itens.AddRange(itens);
        ValorTotal = _itens.Sum(item => item.ValorTotal);
    }

    private void ValidarEdicaoPermitida()
    {
        if (Status == StatusOrcamento.Aceito)
            throw new InvalidOperationException("Orcamento aceito nao pode ser editado.");
    }
}
