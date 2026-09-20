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

    public static ResultadoOcr CriarSucesso(
        Dictionary<string, string> campos,
        decimal conf,
        string? modeloUtilizado = null,
        Dictionary<string, decimal>? confiancasCampos = null)
        => new()
        {
            Sucesso = true,
            CamposExtraidos = campos,
            ConfiancaOcr = conf,
            ModeloUtilizado = modeloUtilizado,
            ConfiancasCampos = confiancasCampos ?? []
        };

    public static ResultadoOcr CriarFalha(string erro, string? modeloUtilizado = null)
        => new() { Sucesso = false, Erro = erro, ModeloUtilizado = modeloUtilizado };
}





