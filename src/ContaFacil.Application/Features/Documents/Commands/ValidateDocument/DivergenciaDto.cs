namespace ContaFacil.Application.Features.Documents.Commands.ValidateDocument;

public record DivergenciaDto(
    string Campo,
    string ValorDocumento,
    string ValorCadastro,
    decimal Similaridade);
