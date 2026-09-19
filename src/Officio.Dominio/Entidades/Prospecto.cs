using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Prospecto : EntidadeAuditavel
{
    public Guid? EmpresaId { get; private set; }
    public Guid? UsuarioResponsavelId { get; private set; }
    public string TipoProspect { get; private set; } = "EMPRESA_EXISTENTE";
    public Guid? StatusProspectId { get; private set; }
    public string RazaoSocial { get; private set; } = default!;
    public string? NomeFantasia { get; private set; }
    public string? Cnpj { get; private set; }
    public string? Cpf { get; private set; }
    public RegimeTributario? RegimeTributario { get; private set; }
    public string? PorteEmpresa { get; private set; }
    public string? CnaePrincipal { get; private set; }
    public DateTime? DataAbertura { get; private set; }
    public StatusProspecto Status { get; private set; }
    public Guid? ClientId { get; private set; }
    public DateTime? ConvertidoEm { get; private set; }
    public string? Email { get; private set; }
    public string? Telefone { get; private set; }
    public string? Whatsapp { get; private set; }
    public string? NomeContato { get; private set; }
    public string? Cep { get; private set; }
    public string? Logradouro { get; private set; }
    public string? Numero { get; private set; }
    public string? Bairro { get; private set; }
    public string? Cidade { get; private set; }
    public string? Uf { get; private set; }
    public string? Origem { get; private set; }
    public string? Observacao { get; private set; }
    public string? MotivoPerda { get; private set; }
    public DateTime? DataReuniao { get; private set; }
    public DateTime? DataUltimoContato { get; private set; }
    public DateTime? DataEncerramento { get; private set; }
    public DateTime? DataConversao { get; private set; }
    public Guid? ResponsavelId { get; private set; }

    // EF Core navigation
    public Empresa? Empresa { get; private set; }
    public Usuario? UsuarioResponsavel { get; private set; }
    public StatusProspect? StatusProspect { get; private set; }
    public FormularioAbertura? FormularioAbertura { get; private set; }
    public ICollection<ProspectoPessoa> Pessoas { get; private set; } = [];
    public ICollection<Orcamento> Budgets { get; private set; } = [];
    public ICollection<Socio> Socios { get; private set; } = [];

    private Prospecto() { }

    public static Prospecto Create(
        Guid tenantId,
        string razaoSocial,
        string? cnpj,
        RegimeTributario? regime)
    {
        var cnpjNormalizado = DocumentoFiscal.SomenteDigitos(cnpj);

        return new Prospecto
        {
            TenantId = tenantId,
            RazaoSocial = razaoSocial,
            Cnpj = cnpjNormalizado,
            RegimeTributario = regime,
            Status = StatusProspecto.EmNegociacao,
            TipoProspect = cnpjNormalizado is null ? "NOVA_EMPRESA" : "EMPRESA_EXISTENTE"
        };
    }

    public void MarkAsConverted(Guid clientId)
    {
        if (Status == StatusProspecto.Convertido)
            throw new InvalidOperationException("Prospecto já foi convertido.");

        Status = StatusProspecto.Convertido;
        ClientId = clientId;
        ConvertidoEm = DateTime.UtcNow;
        DataConversao = ConvertidoEm;

        AddDomainEvent(new EventoProspectoConvertido(Id, clientId, TenantId));
    }

    public Cliente ConverterParaCliente()
    {
        if (Status == StatusProspecto.Convertido)
            throw new InvalidOperationException("Prospecto ja foi convertido.");

        var cliente = Cliente.CreateFromProspect(this);
        foreach (var socio in Socios)
            cliente.AddSocio(Socio.CopyFor(socio, cliente.Id));

        MarkAsConverted(cliente.Id);
        return cliente;
    }

    public void UpdateStatus(StatusProspecto newStatus)
        => Status = newStatus;

    public void VincularEmpresa(Guid empresaId)
        => EmpresaId = empresaId;

    public void DefinirStatusDominio(Guid statusProspectId)
        => StatusProspectId = statusProspectId;
}





