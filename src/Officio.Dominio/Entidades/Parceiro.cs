using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Parceiro : EntidadeBase
{
    public string Nome { get; private set; } = default!;
    public string RazaoSocial { get; private set; } = default!;
    public string? NomeFantasia { get; private set; }
    public string Cnpj { get; private set; } = default!;
    public string? Email { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public ICollection<Empresa> Empresas { get; private set; } = [];
    public ICollection<Usuario> Usuarios { get; private set; } = [];

    private Parceiro() { }

    public static Parceiro Criar(
        string cnpj,
        string razaoSocial,
        string? nomeFantasia = null,
        string? email = null)
    {
        var cnpjNormalizado = ValidarCnpj(cnpj);
        var razaoSocialNormalizada = ValidarRazaoSocial(razaoSocial);

        return new Parceiro
        {
            Nome = razaoSocialNormalizada,
            RazaoSocial = razaoSocialNormalizada,
            NomeFantasia = Limpar(nomeFantasia),
            Cnpj = cnpjNormalizado,
            Email = Limpar(email)
        };
    }

    public static Parceiro CriarDesenvolvimento(
        Guid id,
        string cnpj,
        string razaoSocial,
        string? nomeFantasia = null,
        string? email = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Identificador do parceiro e obrigatorio.", nameof(id));

        var parceiro = Criar(cnpj, razaoSocial, nomeFantasia, email);
        parceiro.Id = id;
        return parceiro;
    }

    public void AtualizarCadastro(
        string cnpj,
        string razaoSocial,
        string? nomeFantasia = null,
        string? email = null)
    {
        Nome = ValidarRazaoSocial(razaoSocial);
        RazaoSocial = Nome;
        Cnpj = ValidarCnpj(cnpj);
        NomeFantasia = Limpar(nomeFantasia);
        Email = Limpar(email);
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AlterarStatus(bool ativo)
    {
        Ativo = ativo;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static string? Limpar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string ValidarCnpj(string cnpj)
    {
        var normalizado = DocumentoFiscal.SomenteDigitos(cnpj);
        if (normalizado?.Length != 14)
            throw new ArgumentException("CNPJ do parceiro deve conter 14 digitos.", nameof(cnpj));

        return normalizado;
    }

    private static string ValidarRazaoSocial(string razaoSocial)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new ArgumentException("Razao social do parceiro e obrigatoria.", nameof(razaoSocial));

        return razaoSocial.Trim();
    }
}
