using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Cliente : EntidadeAuditavel
{
    public Guid EmpresaId { get; private set; }
    public Guid? EnderecoId { get; private set; }
    public Guid StatusClienteId { get; private set; }
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
    public DateTime? InicioRelacionamentoEm { get; private set; }
    public DateTime? EncerradoEm { get; private set; }

    // Navigation
    public Empresa Empresa { get; private set; } = default!;
    public Endereco? Endereco { get; private set; }
    public StatusCliente StatusCliente { get; private set; } = default!;
    public Prospecto? ProspectOrigem { get; private set; }
    public ICollection<Socio> Socios { get; private set; } = [];
    public ICollection<Contrato> Contratos { get; private set; } = [];
    public ICollection<Documento> Documentos { get; private set; } = [];

    private Cliente() { }

    public static Cliente CreateFromProspect(Prospecto prospect, Guid statusClienteId = default)
    {
        return new Cliente
        {
            TenantId = prospect.TenantId,
            EmpresaId = prospect.EmpresaId.GetValueOrDefault(),
            StatusClienteId = statusClienteId,
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
            ProspectOrigemId = prospect.Id,
            InicioRelacionamentoEm = DateTime.UtcNow
        };
    }

    public static Cliente CriarParaEmpresa(Guid tenantId, Guid empresaId, Guid? prospectOrigemId = null, Guid statusClienteId = default)
        => new()
        {
            TenantId = tenantId,
            EmpresaId = empresaId,
            ProspectOrigemId = prospectOrigemId,
            StatusClienteId = statusClienteId,
            RazaoSocial = string.Empty,
            InicioRelacionamentoEm = DateTime.UtcNow
        };

    public void AddSocio(Socio socio) => Socios.Add(socio);

    public void AtualizarDados(
        string razaoSocial,
        string? nomeFantasia,
        string? cnpj,
        string? cpf,
        RegimeTributario? regimeTributario,
        string? email,
        string? telefone,
        string? whatsapp,
        string? cep,
        string? logradouro,
        string? numero,
        string? bairro,
        string? cidade,
        string? uf,
        string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new ArgumentException("Razao social e obrigatoria.", nameof(razaoSocial));

        RazaoSocial = razaoSocial.Trim();
        NomeFantasia = Normalizar(nomeFantasia);
        Cnpj = DocumentoFiscal.SomenteDigitos(cnpj);
        Cpf = DocumentoFiscal.SomenteDigitos(cpf);
        RegimeTributario = regimeTributario;
        Email = Normalizar(email);
        Telefone = Normalizar(telefone);
        Whatsapp = Normalizar(whatsapp);
        Cep = Normalizar(cep);
        Logradouro = Normalizar(logradouro);
        Numero = Normalizar(numero);
        Bairro = Normalizar(bairro);
        Cidade = Normalizar(cidade);
        Uf = Normalizar(uf)?.ToUpperInvariant();
        Observacoes = Normalizar(observacoes);
    }

    public void VincularSciCodigo(string sciCodigo)
    {
        if (string.IsNullOrWhiteSpace(sciCodigo))
            throw new ArgumentException("Codigo SCI e obrigatorio.", nameof(sciCodigo));

        SciCodigo = sciCodigo.Trim();
    }

    public void VincularEndereco(Guid? enderecoId)
        => EnderecoId = enderecoId;

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}





