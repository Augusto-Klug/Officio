using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Prospecto : EntidadeAuditavel
{
    public Guid? EmpresaId { get; private set; }
    public Guid? MunicipioId { get; private set; }
    public Guid? UsuarioResponsavelId { get; private set; }
    public string TipoProspect { get; private set; } = "EMPRESA_EXISTENTE";
    public Guid? StatusProspectId { get; private set; }
    public string RazaoSocial { get; private set; } = default!;
    public string? NomeFantasia { get; private set; }
    public string? Cnpj { get; private set; }
    public string? Cpf { get; private set; }
    public string? InscricaoEstadual { get; private set; }
    public string? InscricaoMunicipal { get; private set; }
    public string? TipoEmpresa { get; private set; }
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
    public string? Complemento { get; private set; }
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
    public Municipio? Municipio { get; private set; }
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

    public Cliente ConverterParaCliente(Guid statusClienteId = default)
    {
        if (Status == StatusProspecto.Convertido)
            throw new InvalidOperationException("Prospecto ja foi convertido.");

        var cliente = Cliente.CreateFromProspect(this, statusClienteId);
        foreach (var socio in Socios)
            cliente.AddSocio(Socio.CopyFor(socio, cliente.Id));

        MarkAsConverted(cliente.Id);
        return cliente;
    }

    public void UpdateStatus(StatusProspecto newStatus)
        => Status = newStatus;

    public void AtualizarDadosCadastrais(
        string? nomeFantasia,
        string? cpf,
        string? porteEmpresa,
        string? cnaePrincipal,
        DateTime? dataAbertura,
        string? email,
        string? telefone,
        string? whatsapp,
        string? nomeContato,
        string? cep,
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? uf,
        string? origem,
        Guid? responsavelId,
        string? inscricaoEstadual,
        string? inscricaoMunicipal,
        string? tipoEmpresa)
    {
        NomeFantasia = Normalizar(nomeFantasia);
        Cpf = DocumentoFiscal.SomenteDigitos(cpf);
        PorteEmpresa = Normalizar(porteEmpresa);
        CnaePrincipal = DocumentoFiscal.SomenteDigitos(cnaePrincipal);
        DataAbertura = dataAbertura;
        Email = Normalizar(email);
        Telefone = Normalizar(telefone);
        Whatsapp = Normalizar(whatsapp);
        NomeContato = Normalizar(nomeContato);
        Cep = DocumentoFiscal.SomenteDigitos(cep);
        Logradouro = Normalizar(logradouro);
        Numero = Normalizar(numero);
        Complemento = Normalizar(complemento);
        Bairro = Normalizar(bairro);
        Cidade = Normalizar(cidade);
        Uf = Normalizar(uf)?.ToUpperInvariant();
        Origem = Normalizar(origem);
        ResponsavelId = responsavelId;
        InscricaoEstadual = Normalizar(inscricaoEstadual);
        InscricaoMunicipal = Normalizar(inscricaoMunicipal);
        TipoEmpresa = Normalizar(tipoEmpresa);
    }

    public void VincularMunicipio(Guid municipioId, string cidade, string uf)
    {
        MunicipioId = municipioId;
        Cidade = cidade.Trim();
        Uf = uf.Trim().ToUpperInvariant();
    }

    public void VincularEmpresa(Guid empresaId)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("Empresa e obrigatoria.", nameof(empresaId));

        EmpresaId = empresaId;
    }

    public void DefinirStatusDominio(Guid statusProspectId)
        => StatusProspectId = statusProspectId;

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}





