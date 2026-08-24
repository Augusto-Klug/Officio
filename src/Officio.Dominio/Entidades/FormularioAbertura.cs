using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class FormularioAbertura : EntidadeBase
{
    private readonly List<FormularioAberturaAtividade> _atividades = [];

    public Guid ProspectId { get; private set; }
    public Guid? PessoaResponsavelId { get; private set; }
    public Guid? EnderecoId { get; private set; }
    public Guid StatusFormularioId { get; private set; }
    public string? NomeEmpresaPretendido { get; private set; }
    public string? NomeFantasiaPretendido { get; private set; }
    public string? CadastroIptu { get; private set; }
    public string? NomeEdificacao { get; private set; }
    public string? ProprietarioImovelNome { get; private set; }
    public string? ProprietarioImovelCpfCnpj { get; private set; }
    public decimal? AreaTotalEdificacaoM2 { get; private set; }
    public decimal? AreaOcupadaEmpresaM2 { get; private set; }
    public decimal? CapitalSocialPrevisto { get; private set; }
    public string? EmailEmpresaPretendido { get; private set; }
    public string? TelefoneEmpresaPretendido { get; private set; }
    public int? NumeroFuncionariosPrevisto { get; private set; }
    public int? NumeroProLaboresPrevisto { get; private set; }
    public int? LotacaoMaximaPublico { get; private set; }
    public string? HorarioFuncionamento { get; private set; }
    public int? DiasTrabalhadosMes { get; private set; }
    public string? SituacaoImovel { get; private set; }
    public int? NumeroMaquinas { get; private set; }
    public string? TipoMaquinas { get; private set; }
    public string? EnquadramentoPretendido { get; private set; }
    public string? Observacoes { get; private set; }
    public string? TipoEmpresaPretendido { get; private set; }
    public string? RegimeTributarioPretendido { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public Prospecto Prospecto { get; private set; } = default!;
    public Pessoa? PessoaResponsavel { get; private set; }
    public Endereco? Endereco { get; private set; }
    public StatusFormulario StatusFormulario { get; private set; } = default!;
    public IReadOnlyCollection<FormularioAberturaAtividade> Atividades => _atividades.AsReadOnly();

    private FormularioAbertura() { }

    public static FormularioAbertura Criar(Guid prospectId, Guid statusFormularioId)
        => new() { ProspectId = prospectId, StatusFormularioId = statusFormularioId };
}
