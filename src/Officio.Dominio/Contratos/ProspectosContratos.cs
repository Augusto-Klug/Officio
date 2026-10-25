using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Contratos;

public record CriarProspectoRequisicao(
    string RazaoSocial,
    string? Cnpj,
    RegimeTributario? RegimeTributario,
    IReadOnlyCollection<CriarSocioRequisicao>? Socios,
    string? NomeFantasia = null,
    string? Cpf = null,
    string? PorteEmpresa = null,
    string? CnaePrincipal = null,
    DateTime? DataAbertura = null,
    string? Email = null,
    string? Telefone = null,
    string? Whatsapp = null,
    string? NomeContato = null,
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Complemento = null,
    string? Bairro = null,
    string? Cidade = null,
    string? Uf = null,
    string? MunicipioCodigoIbge = null,
    string? Origem = null,
    Guid? ResponsavelId = null,
    string? InscricaoEstadual = null,
    string? InscricaoMunicipal = null,
    string? TipoEmpresa = null);

public record EnderecoSocioRequisicao(string? Cep = null, string? Logradouro = null, string? Numero = null,
    string? Complemento = null, string? Bairro = null, string? Cidade = null, string? Uf = null,
    string? MunicipioCodigoIbge = null);
public record CriarSocioRequisicao(string? Nome = null, string? Cpf = null, decimal? Participacao = null,
    Guid? Id = null, string? Profissao = null, Guid? TipoRelacaoId = null,
    EnderecoSocioRequisicao? Endereco = null, Guid? PessoaId = null);

public record ProspectoDetalheDto(Guid Id, string Status, CriarProspectoRequisicao Dados);
public record TipoRelacaoSocietariaDto(Guid Id, string Codigo, string Descricao);

public static class ProspectoErros
{
    public const string NaoEncontrado = "Prospecto nao encontrado.";
    public const string ForaDaEtapaInicial = "Somente prospectos em negociacao, ainda sem conversao ou empresa vinculada, podem ser editados ou excluidos.";
    public const string PossuiVinculos = "O prospecto possui orcamentos, contratos ou outros registros vinculados e nao pode ser excluido.";
}

public record ProspectoCriadoResultado(Guid Id, string RazaoSocial);

public record ObterProspectosFiltro(
    int Pagina,
    int TamanhoPagina,
    string? Status,
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
