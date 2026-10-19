using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Interfaces;

public interface IServicoOcr
{
    Task<ResultadoOcr> AnalisarDocumentoAsync(Stream documentStream, TipoDocumento type, CancellationToken ct = default);
}

public record ResultadoOcr
{
    public bool Sucesso { get; init; }
    public Dictionary<string, string> CamposExtraidos { get; init; } = [];
    public decimal ConfiancaOcr { get; init; }
    public string? ModeloUtilizado { get; init; }
    public Dictionary<string, decimal> ConfiancasCampos { get; init; } = [];
    public string? Erro { get; init; }
    public string CategoriaResultado { get; init; } = "OCR_CONCLUIDO";
    public string? CodigoErro { get; init; }
    public int? StatusHttp { get; init; }
    public string? IdRequisicao { get; init; }
    public string? DiagnosticoTecnico { get; init; }

    public static ResultadoOcr CriarSucesso(
        Dictionary<string, string> campos,
        decimal conf,
        string? modeloUtilizado = null,
        Dictionary<string, decimal>? confiancasCampos = null,
        string? idRequisicao = null,
        int? statusHttp = null)
        => new()
        {
            Sucesso = true,
            CamposExtraidos = campos,
            ConfiancaOcr = conf,
            ModeloUtilizado = modeloUtilizado,
            ConfiancasCampos = confiancasCampos ?? [],
            IdRequisicao = idRequisicao,
            StatusHttp = statusHttp
        };

    public static ResultadoOcr CriarFalha(
        string erro,
        string? modeloUtilizado = null,
        string categoriaResultado = "FALHA_INTEGRACAO",
        string? codigoErro = null,
        int? statusHttp = null,
        string? idRequisicao = null,
        string? diagnosticoTecnico = null)
        => new()
        {
            Sucesso = false,
            Erro = erro,
            ModeloUtilizado = modeloUtilizado,
            CategoriaResultado = categoriaResultado,
            CodigoErro = codigoErro,
            StatusHttp = statusHttp,
            IdRequisicao = idRequisicao,
            DiagnosticoTecnico = diagnosticoTecnico
        };
}





