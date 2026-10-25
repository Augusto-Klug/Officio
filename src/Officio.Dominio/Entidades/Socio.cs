using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Socio : EntidadeAuditavel
{
    // Projeções de compatibilidade: Pessoa é a única fonte dos dados pessoais.
    public string Nome => Pessoa.Nome;
    public string? Cpf => Pessoa.Cpf;
    public Guid PessoaId { get; private set; }
    public Pessoa Pessoa { get; private set; } = default!;
    public Guid? TipoRelacaoId { get; private set; }
    public TipoRelacao? TipoRelacao { get; private set; }
    public decimal? Participacao { get; private set; }
    public Guid? ProspectId { get; private set; }
    public Guid? ClientId { get; private set; }

    private Socio() { }

    public static Socio Create(Guid tenantId, string? nome, string? cpf, decimal? participacao, Guid? prospectId = null, Guid? clientId = null)
    {
        return new Socio
        {
            TenantId = tenantId,
            Pessoa = Pessoa.Criar(nome ?? "", DocumentoFiscal.SomenteDigitos(cpf)),
            Participacao = participacao,
            ProspectId = prospectId,
            ClientId = clientId
        }.VincularPessoaId();
    }

    public static Socio CopyFor(Socio source, Guid clientId)
    {
        return new Socio
        {
            TenantId = source.TenantId,
            Pessoa = source.Pessoa,
            PessoaId = source.PessoaId,
            TipoRelacao = source.TipoRelacao,
            TipoRelacaoId = source.TipoRelacaoId,
            Participacao = source.Participacao,
            ClientId = clientId
        };
    }

    private Socio VincularPessoaId() { PessoaId = Pessoa.Id; return this; }

    public void AtualizarVinculo(decimal? participacao, TipoRelacao? tipoRelacao)
    {
        if (participacao is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(participacao));
        Participacao = participacao;
        TipoRelacao = tipoRelacao;
        TipoRelacaoId = tipoRelacao?.Id;
    }

    public void AtualizarDados(string? nome, string? cpf, decimal? participacao)
    {
        Pessoa.AtualizarDadosSocietarios(nome, cpf, Pessoa.Profissao, Pessoa.Endereco);
        Participacao = participacao;
    }
}





