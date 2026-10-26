using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Pessoa : EntidadeBase
{
    public Guid? EnderecoId { get; private set; }
    public string Nome { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public string? Rg { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public string? Sexo { get; private set; }
    public string? EstadoCivil { get; private set; }
    public string? Profissao { get; private set; }
    public string? Naturalidade { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? TituloEleitor { get; private set; }
    public string? ReciboUltimaIrpf { get; private set; }
    public string? CartorioAssinatura { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public Endereco? Endereco { get; private set; }

    private Pessoa() { }

    public void AtualizarDadosSocietarios(string? nome, string? cpf, string? profissao, Endereco? endereco)
    {
        Nome = nome?.Trim() ?? "";
        Cpf = DocumentoFiscal.SomenteDigitos(cpf);
        Profissao = string.IsNullOrWhiteSpace(profissao) ? null : profissao.Trim();
        Endereco = endereco;
        EnderecoId = endereco?.Id;
        AtualizadoEm = DateTime.UtcNow;
    }

    public static Pessoa Criar(string nome, string? cpf = null, string? telefone = null, string? email = null)
        => new() { Nome = nome, Cpf = cpf, Telefone = telefone, Email = email };
}
