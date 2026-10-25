using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Empresa : EntidadeAuditavel
{
    public Guid? EnderecoId { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Cnpj { get; private set; } = default!;
    public string RazaoSocial { get; private set; } = default!;
    public string? NomeFantasia { get; private set; }
    public string? InscricaoEstadual { get; private set; }
    public string? InscricaoMunicipal { get; private set; }
    public string? Telefone { get; private set; }
    public string? Whatsapp { get; private set; }
    public string? Email { get; private set; }
    public string? TipoEmpresa { get; private set; }
    public string? RegimeTributario { get; private set; }
    public int QuantidadeFuncionariosAtual { get; private set; }
    public int QuantidadeProLaboresAtual { get; private set; }
    public bool Ativo { get; private set; } = true;
    public Endereco? Endereco { get; private set; }
    public Parceiro Parceiro { get; private set; } = default!;
    public ICollection<EmpresaPessoa> Pessoas { get; private set; } = [];
    public ICollection<Admissao> Admissoes { get; private set; } = [];
    public ICollection<EmpresaFuncionario> Funcionarios { get; private set; } = [];

    private Empresa() { }

    public static Empresa Create(Guid parceiroId, string nome, string? cnpj)
    {
        return new Empresa
        {
            TenantId = parceiroId,
            Nome = nome,
            RazaoSocial = nome,
            Cnpj = cnpj ?? string.Empty
        };
    }

    public static Empresa CriarConstituida(
        Guid parceiroId,
        string cnpj,
        string razaoSocial,
        string? nomeFantasia = null,
        int quantidadeFuncionariosAtual = 0,
        int quantidadeProLaboresAtual = 0,
        string? email = null,
        string? regimeTributario = null,
        string? inscricaoEstadual = null,
        string? inscricaoMunicipal = null,
        string? telefone = null,
        string? whatsapp = null,
        string? tipoEmpresa = null)
    {
        return new Empresa
        {
            TenantId = parceiroId,
            Nome = razaoSocial,
            Cnpj = cnpj,
            RazaoSocial = razaoSocial,
            NomeFantasia = nomeFantasia,
            Email = email,
            RegimeTributario = regimeTributario,
            InscricaoEstadual = inscricaoEstadual,
            InscricaoMunicipal = inscricaoMunicipal,
            Telefone = telefone,
            Whatsapp = whatsapp,
            TipoEmpresa = tipoEmpresa,
            QuantidadeFuncionariosAtual = quantidadeFuncionariosAtual,
            QuantidadeProLaboresAtual = quantidadeProLaboresAtual
        };
    }

    public static Empresa CriarDoProspecto(Prospecto prospecto)
    {
        if (string.IsNullOrWhiteSpace(prospecto.Cnpj))
            throw new ArgumentException("Prospecto deve possuir CNPJ para constituir a empresa.", nameof(prospecto));

        return CriarConstituida(
            prospecto.TenantId,
            DocumentoFiscal.SomenteDigitos(prospecto.Cnpj)!,
            prospecto.RazaoSocial,
            prospecto.NomeFantasia,
            email: prospecto.Email,
            regimeTributario: prospecto.RegimeTributario?.ToString(),
            inscricaoEstadual: prospecto.InscricaoEstadual,
            inscricaoMunicipal: prospecto.InscricaoMunicipal,
            telefone: prospecto.Telefone,
            whatsapp: prospecto.Whatsapp,
            tipoEmpresa: prospecto.TipoEmpresa);
    }

    public void AtualizarQuantidadesOperacionais(int quantidadeFuncionariosAtual, int quantidadeProLaboresAtual)
    {
        QuantidadeFuncionariosAtual = quantidadeFuncionariosAtual;
        QuantidadeProLaboresAtual = quantidadeProLaboresAtual;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void IncrementarQuantidadeFuncionarios()
    {
        QuantidadeFuncionariosAtual++;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AlterarStatus(bool ativo)
    {
        Ativo = ativo;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void VincularEndereco(Guid? enderecoId)
    {
        EnderecoId = enderecoId;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AtualizarDadosCadastrais(
        string razaoSocial,
        string? nomeFantasia,
        string? cnpj,
        string? email,
        string? telefone,
        string? whatsapp,
        string? regimeTributario)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new ArgumentException("Razao social e obrigatoria.", nameof(razaoSocial));

        RazaoSocial = razaoSocial.Trim();
        Nome = RazaoSocial;
        NomeFantasia = Normalizar(nomeFantasia);
        Cnpj = DocumentoFiscal.SomenteDigitos(cnpj) ?? string.Empty;
        Email = Normalizar(email);
        Telefone = Normalizar(telefone);
        Whatsapp = Normalizar(whatsapp);
        RegimeTributario = Normalizar(regimeTributario);
        AtualizadoEm = DateTime.UtcNow;
    }

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}





