namespace Officio.Dominio.Contratos;

public record CriarSocioRequisicao(
    string? Nome = null,
    string? Cpf = null,
    decimal? Participacao = null,
    Guid? Id = null,
    string? Profissao = null,
    Guid? TipoRelacaoId = null,
    EnderecoSocioRequisicao? Endereco = null,
    Guid? PessoaId = null);
