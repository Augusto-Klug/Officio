using Officio.ViewModels.Enumeradores;

namespace Officio.ViewModels.Prospectos;

public class CriarProspectoViewModel
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string? Cnpj { get; set; }
    public string? Cpf { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? TipoEmpresa { get; set; }
    public RegimeTributarioViewModel? RegimeTributario { get; set; }
    public string? PorteEmpresa { get; set; }
    public string? CnaePrincipal { get; set; }
    public DateTime? DataAbertura { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public string? NomeContato { get; set; }
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? MunicipioCodigoIbge { get; set; }
    public string? Origem { get; set; }
    public Guid? ResponsavelId { get; set; }
    public List<CriarSocioViewModel>? Socios { get; set; }
}

public class CriarSocioViewModel
{
    public Guid? Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public decimal Participacao { get; set; }
}
