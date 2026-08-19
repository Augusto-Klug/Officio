using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Orcamento : EntidadeAuditavel
{
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
    public Prospecto Prospecto { get; private set; } = default!;
    public ICollection<ItemOrcamento> Itens { get; private set; } = [];

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
        if (Status == StatusOrcamento.Aceito)
            throw new InvalidOperationException("Orcamento aceito nao pode ser editado.");

        Titulo = titulo;
        DiaVencimento = diaVencimento;
        Observacoes = observacoes;
    }

    public void Aceitar() => Status = StatusOrcamento.Aceito;

    public void Recusar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo da recusa e obrigatorio.", nameof(motivo));

        Status = StatusOrcamento.Recusado;
    }

    public void AlterarStatus(StatusOrcamento status) => Status = status;

    public void AdicionarItem(ItemOrcamento item)
    {
        Itens.Add(item);
        ValorTotal = Itens.Sum(i => i.ValorTotal);
    }
}
