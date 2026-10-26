using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Endereco : EntidadeBase
{
    public Guid? MunicipioId { get; private set; }
    public Guid? EstadoId { get; private set; }
    public Estado? Estado { get; private set; }
    public string? Cep { get; private set; }
    public string Logradouro { get; private set; } = default!;
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string? Bairro { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public Municipio Municipio { get; private set; } = default!;

    private Endereco() { }

    public void AtualizarParcial(Guid? municipioId, Guid? estadoId, string? logradouro, string? cep,
        string? numero, string? bairro, string? complemento)
    {
        MunicipioId = municipioId;
        EstadoId = estadoId;
        Logradouro = logradouro?.Trim() ?? "";
        Cep = DocumentoFiscal.SomenteDigitos(cep);
        Numero = numero?.Trim(); Bairro = bairro?.Trim(); Complemento = complemento?.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Atualizar(Guid municipioId, string logradouro, string? cep, string? numero, string? bairro, string? complemento)
    {
        MunicipioId = municipioId;
        Logradouro = logradouro;
        Cep = cep;
        Numero = numero;
        Bairro = bairro;
        Complemento = complemento;
        AtualizadoEm = DateTime.UtcNow;
    }

    public static Endereco Criar(Guid? municipioId, string logradouro, string? cep = null, string? numero = null, string? bairro = null, string? complemento = null)
        => new()
        {
            MunicipioId = municipioId,
            Logradouro = logradouro,
            Cep = cep,
            Numero = numero,
            Bairro = bairro,
            Complemento = complemento
        };
}
