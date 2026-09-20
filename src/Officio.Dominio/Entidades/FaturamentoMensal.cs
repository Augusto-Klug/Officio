using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class FaturamentoMensal : EntidadeAuditavel
{
    public Guid ContratoId { get; private set; }
    public Guid StatusFaturamentoId { get; private set; }
    public DateOnly Competencia { get; private set; }
    public int QuantidadeFuncionarios { get; private set; }
    public int QuantidadeProLabores { get; private set; }
    public int QuantidadeFolhas { get; private set; }
    public decimal ValorHonorarioBase { get; private set; }
    public decimal ValorPorFolha { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime CalculadoEm { get; private set; } = DateTime.UtcNow;
    public Contrato Contrato { get; private set; } = default!;
    public StatusFaturamento StatusFaturamento { get; private set; } = default!;

    private FaturamentoMensal() { }

    public static FaturamentoMensal Criar(
        Guid tenantId,
        Guid contratoId,
        Guid statusFaturamentoId,
        DateOnly competencia,
        int quantidadeFuncionarios,
        int quantidadeProLabores,
        decimal valorHonorarioBase,
        decimal valorPorFolha)
    {
        var quantidadeFolhas = quantidadeFuncionarios + quantidadeProLabores;

        return new FaturamentoMensal
        {
            TenantId = tenantId,
            ContratoId = contratoId,
            StatusFaturamentoId = statusFaturamentoId,
            Competencia = competencia,
            QuantidadeFuncionarios = quantidadeFuncionarios,
            QuantidadeProLabores = quantidadeProLabores,
            QuantidadeFolhas = quantidadeFolhas,
            ValorHonorarioBase = valorHonorarioBase,
            ValorPorFolha = valorPorFolha,
            ValorTotal = valorHonorarioBase + quantidadeFolhas * valorPorFolha
        };
    }

    public void Fechar(Guid statusFechadoId)
    {
        if (StatusFaturamento.Codigo != "ABERTO")
            throw new InvalidOperationException("Somente faturamentos abertos podem ser fechados.");

        StatusFaturamentoId = statusFechadoId;
    }

    public void Cancelar(Guid statusCanceladoId)
    {
        if (StatusFaturamento.Codigo == "FECHADO")
            throw new InvalidOperationException("Faturamento fechado nao pode ser cancelado.");
        if (StatusFaturamento.Codigo == "CANCELADO")
            throw new InvalidOperationException("Faturamento ja esta cancelado.");

        StatusFaturamentoId = statusCanceladoId;
    }
}
