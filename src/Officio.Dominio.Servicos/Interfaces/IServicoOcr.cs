using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Servicos.Interfaces;

public interface IServicoOcr
{
    Task<ResultadoOcr> AnalisarDocumentoAsync(Stream documentStream, TipoDocumento type, CancellationToken ct = default);
}

public record ResultadoOcr
{
    public bool Sucesso { get; init; }
    public Dictionary<string, string> CamposExtraidos { get; init; } = [];
    public decimal ConfiancaOcr { get; init; }
    public string? Erro { get; init; }

    public static ResultadoOcr CriarSucesso(Dictionary<string, string> campos, decimal conf)
        => new() { Sucesso = true, CamposExtraidos = campos, ConfiancaOcr = conf };

    public static ResultadoOcr CriarFalha(string erro)
        => new() { Sucesso = false, Erro = erro };
}





