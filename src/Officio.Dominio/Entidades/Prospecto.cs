using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Prospecto : EntidadeAuditavel
{
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
    public Guid? ResponsavelId { get; private set; }

    // EF Core navigation
    public ICollection<Orcamento> Budgets { get; private set; } = [];
    public ICollection<Socio> Socios { get; private set; } = [];

    private Prospecto() { }

    public static Prospecto Create(
        Guid tenantId,
        string razaoSocial,
        string? cnpj,
        RegimeTributario? regime)
    {
        return new Prospecto
        {
            TenantId = tenantId,
            RazaoSocial = razaoSocial,
            Cnpj = cnpj,
            RegimeTributario = regime,
            Status = StatusProspecto.EmNegociacao
        };
    }

    public void MarkAsConverted(Guid clientId)
    {
        if (Status == StatusProspecto.Convertido)
            throw new InvalidOperationException("Prospecto jÃ¡ foi convertido.");

        Status = StatusProspecto.Convertido;
        ClientId = clientId;
        ConvertidoEm = DateTime.UtcNow;

        AddDomainEvent(new EventoProspectoConvertido(Id, clientId, TenantId));
    }

    public void UpdateStatus(StatusProspecto newStatus)
        => Status = newStatus;
}





