using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Cliente : EntidadeAuditavel
{
    public string RazaoSocial { get; private set; } = default!;
    public string? NomeFantasia { get; private set; }
    public string? Cnpj { get; private set; }
    public string? Cpf { get; private set; }
    public RegimeTributario? RegimeTributario { get; private set; }
    public string? SciCodigo { get; private set; }
    public bool Ativo { get; private set; } = true;
    public string? Email { get; private set; }
    public string? Telefone { get; private set; }
    public string? Whatsapp { get; private set; }
    public string? Cep { get; private set; }
    public string? Logradouro { get; private set; }
    public string? Numero { get; private set; }
    public string? Bairro { get; private set; }
    public string? Cidade { get; private set; }
    public string? Uf { get; private set; }
    public string? Observacoes { get; private set; }
    public Guid? ProspectOrigemId { get; private set; }

    // Navigation
    public ICollection<Socio> Socios { get; private set; } = [];
    public ICollection<Contrato> Contratos { get; private set; } = [];
    public ICollection<Documento> Documentos { get; private set; } = [];

    private Cliente() { }

    public static Cliente CreateFromProspect(Prospecto prospect)
    {
        return new Cliente
        {
            TenantId = prospect.TenantId,
            RazaoSocial = prospect.RazaoSocial,
            NomeFantasia = prospect.NomeFantasia,
            Cnpj = prospect.Cnpj,
            Cpf = prospect.Cpf,
            RegimeTributario = prospect.RegimeTributario,
            Email = prospect.Email,
            Telefone = prospect.Telefone,
            Whatsapp = prospect.Whatsapp,
            Cep = prospect.Cep,
            Logradouro = prospect.Logradouro,
            Numero = prospect.Numero,
            Bairro = prospect.Bairro,
            Cidade = prospect.Cidade,
            Uf = prospect.Uf,
            ProspectOrigemId = prospect.Id
        };
    }

    public void AddSocio(Socio socio) => Socios.Add(socio);

    public void VincularSciCodigo(string sciCodigo) => SciCodigo = sciCodigo;
}





