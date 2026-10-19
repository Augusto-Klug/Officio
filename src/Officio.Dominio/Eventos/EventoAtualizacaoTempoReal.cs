namespace Officio.Dominio.Eventos;

public static class EntidadesAtualizacaoTempoReal
{
    public const string Contrato = "contrato";
    public const string Admissao = "admissao";
    public const string Prospecto = "prospecto";
    public const string Orcamento = "orcamento";
    public const string CatalogoOrcamento = "catalogo-orcamento";
    public const string Cliente = "cliente";
    public const string Notificacao = "notificacao";
    public const string Faturamento = "faturamento";
    public const string Dashboard = "dashboard";
}

public sealed record EventoAtualizacaoTempoReal(
    Guid ParceiroId,
    string Entidade,
    Guid? Id = null);
