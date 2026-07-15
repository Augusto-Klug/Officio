using Officio.Dominio.Entidades;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Orcamento : EntidadeAuditavel
{
    public int Numero { get; private set; }
    public string? Titulo { get; private set; }
    public Guid ProspectId { get; private set; }
    public decimal ValorMensalidade { get; private set; }
    public decimal ValorPorFuncionario { get; private set; }
    public int QtdFuncionarios { get; private set; }
    public decimal ValorTaxasExtras { get; private set; }
    public decimal Desconto { get; private set; }
    public decimal ValorTotal { get; private set; }
    public int DiaVencimento { get; private set; } = 5;
    public int? VigenciaMeses { get; private set; }
    public DateTime? DataInicio { get; private set; }
    public DateTime? DataExpiracaoOrcamento { get; private set; }
    public string? Observacoes { get; private set; }
    public StatusOrcamento Status { get; private set; } = StatusOrcamento.Rascunho;
    public Guid? ContractId { get; private set; }

    // Navigation
    public Prospecto Prospecto { get; private set; } = default!;
    public ICollection<ItemOrcamento> Itens { get; private set; } = [];

    private Orcamento() { }

    public static Orcamento Create(
        Guid tenantId,
        Guid prospectId,
        string? titulo,
        decimal valorMensalidade,
        decimal valorPorFuncionario,
        int qtdFuncionarios,
        decimal valorTaxasExtras,
        decimal desconto,
        int diaVencimento)
    {
        var budget = new Orcamento
        {
            TenantId = tenantId,
            ProspectId = prospectId,
            Titulo = titulo,
            ValorMensalidade = valorMensalidade,
            ValorPorFuncionario = valorPorFuncionario,
            QtdFuncionarios = qtdFuncionarios,
            ValorTaxasExtras = valorTaxasExtras,
            Desconto = desconto,
            DiaVencimento = diaVencimento
        };

        budget.CalcularTotal();
        return budget;
    }

    public void CalcularTotal()
    {
        ValorTotal = ValorMensalidade
            + (ValorPorFuncionario * QtdFuncionarios)
            + ValorTaxasExtras
            - Desconto;
    }

    public void UpdateStatus(StatusOrcamento newStatus) => Status = newStatus;

    public void MarkAsConverted(Guid contractId)
    {
        Status = StatusOrcamento.Convertido;
        ContractId = contractId;
    }
}





