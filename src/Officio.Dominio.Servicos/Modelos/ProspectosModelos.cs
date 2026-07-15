using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Servicos.Modelos;

public record CriarProspectoRequisicao(
    string RazaoSocial,
    string? Cnpj,
    RegimeTributario? RegimeTributario,
    IReadOnlyCollection<CriarSocioRequisicao>? Socios);

public record CriarSocioRequisicao(string Nome, string? Cpf, decimal Participacao);

public record ProspectoCriadoResultado(Guid Id, string RazaoSocial);

public record ObterProspectosFiltro(
    int Pagina,
    int TamanhoPagina,
    string? Busca,
    Guid? ResponsavelId);

public record ProspectoResumo(
    Guid Id,
    string RazaoSocial,
    string? Cnpj,
    string Status,
    string? ResponsavelNome,
    DateTime CriadoEm);

public record ResultadoPaginado<T>(
    List<T> Dados,
    int TotalRegistros,
    int Pagina,
    int TamanhoPagina,
    int TotalPaginas);
