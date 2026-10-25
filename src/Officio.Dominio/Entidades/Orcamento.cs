using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Eventos;

namespace Officio.Dominio.Entidades;

public class Orcamento : EntidadeAuditavel
{
    private List<ItemOrcamento> _itens = [];
    private readonly List<OrcamentoVersao> _versoes = [];

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
    public IReadOnlyCollection<OrcamentoVersao> Versoes => _versoes.AsReadOnly();

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

        ValidarStatusNaoTerminal();

        Status = StatusOrcamento.Aceito;
        AddDomainEvent(new EventoOrcamentoAceito(Id, ProspectId, TenantId));
    }

    public void Recusar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo da recusa e obrigatorio.", nameof(motivo));

        ValidarStatusNaoTerminal();

        Status = StatusOrcamento.Recusado;
        MotivoRecusa = motivo;
    }

    public void AlterarStatus(StatusOrcamento status)
    {
        ValidarStatusNaoTerminal();

        if (status is StatusOrcamento.Aceito or StatusOrcamento.Recusado or StatusOrcamento.Substituido)
            throw new InvalidOperationException("Use Aceitar ou Recusar para definir o status terminal do orcamento.");

        Status = status;
    }

    public void AdicionarItem(ItemOrcamento item)
    {
        ValidarEdicaoPermitida();
        ValidarNaturezasUnicas(_itens.Append(item));
        _itens.Add(item);
        ValorTotal = _itens.Sum(i => i.ValorTotal);
    }

    public void SubstituirItens(IEnumerable<ItemOrcamento> itens)
    {
        ValidarEdicaoPermitida();
        var novosItens = itens.ToList();
        ValidarNaturezasUnicas(novosItens);
        _itens.Clear();
        _itens.AddRange(novosItens);
        ValorTotal = _itens.Sum(item => item.ValorTotal);
    }

    public void Substituir()
    {
        if (Status != StatusOrcamento.Aceito)
            throw new InvalidOperationException("Somente um orcamento aceito pode ser substituido.");

        Status = StatusOrcamento.Substituido;
    }

    public void AdicionarVersao(OrcamentoVersao versao)
        => _versoes.Add(versao);

    private void ValidarEdicaoPermitida()
    {
        if (Status is not (StatusOrcamento.Rascunho or StatusOrcamento.EmRevisao))
            throw new InvalidOperationException("Orcamento nao pode ser editado no status atual.");
    }

    private void ValidarStatusNaoTerminal()
    {
        if (Status is StatusOrcamento.Aceito or StatusOrcamento.Recusado or StatusOrcamento.Expirado or StatusOrcamento.Substituido)
            throw new InvalidOperationException("Orcamento em status terminal nao pode ser alterado.");
    }

    private static void ValidarNaturezasUnicas(IEnumerable<ItemOrcamento> itens)
    {
        var naturezas = itens
            .GroupBy(item => item.NaturezaCobranca)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());

        if (naturezas.GetValueOrDefault(NaturezaCobrancaItemOrcamento.HonorarioMensal) > 1)
            throw new InvalidOperationException(OrcamentoErros.HonorarioMensalDuplicado);

        if (naturezas.GetValueOrDefault(NaturezaCobrancaItemOrcamento.PorFolha) > 1)
            throw new InvalidOperationException(OrcamentoErros.PorFolhaDuplicado);
    }
}
