namespace Officio.Dominio.Contratos.Dtos;

public record ResultadoEnvioDocumentoDto(
    Guid Id,
    string NomeArquivo,
    decimal QualidadeEstimada,
    bool Legivel,
    string Message);

public record ValidationResultDto(
    bool Valido,
    decimal ConfiancaOcr,
    List<DivergenciaDto> Divergencias,
    bool FallbackManual,
    string? Message,
    string? DiagnosticoTecnicoOcr = null)
{
    public string? Mensagem => Message;
}

public record DivergenciaDto(
    string Campo,
    string ValorDocumento,
    string ValorCadastro,
    decimal Similaridade);
