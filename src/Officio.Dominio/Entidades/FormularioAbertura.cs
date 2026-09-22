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

    public void Atualizar(
        Guid? pessoaResponsavelId,
        Guid? enderecoId,
        string? nomeEmpresaPretendido,
        string? nomeFantasiaPretendido,
        string? cadastroIptu,
        string? nomeEdificacao,
        string? proprietarioImovelNome,
        string? proprietarioImovelCpfCnpj,
        decimal? areaTotalEdificacaoM2,
        decimal? areaOcupadaEmpresaM2,
        decimal? capitalSocialPrevisto,
        string? emailEmpresaPretendido,
        string? telefoneEmpresaPretendido,
        int? numeroFuncionariosPrevisto,
        int? numeroProLaboresPrevisto,
        int? lotacaoMaximaPublico,
        string? horarioFuncionamento,
        int? diasTrabalhadosMes,
        string? situacaoImovel,
        int? numeroMaquinas,
        string? tipoMaquinas,
        string? enquadramentoPretendido,
        string? observacoes,
        string? tipoEmpresaPretendido,
        string? regimeTributarioPretendido)
    {
        if (areaTotalEdificacaoM2 < 0 || areaOcupadaEmpresaM2 < 0 || capitalSocialPrevisto < 0)
            throw new ArgumentOutOfRangeException(nameof(areaTotalEdificacaoM2), "Areas e capital social nao podem ser negativos.");
        if (areaTotalEdificacaoM2.HasValue && areaOcupadaEmpresaM2 > areaTotalEdificacaoM2)
            throw new ArgumentException("Area ocupada nao pode ser maior que a area total.", nameof(areaOcupadaEmpresaM2));
        if (numeroFuncionariosPrevisto < 0 || numeroProLaboresPrevisto < 0 || lotacaoMaximaPublico < 0 ||
            diasTrabalhadosMes is < 1 or > 31 || numeroMaquinas < 0)
            throw new ArgumentOutOfRangeException(nameof(numeroFuncionariosPrevisto), "Valores numericos informados sao invalidos.");

        PessoaResponsavelId = pessoaResponsavelId;
        EnderecoId = enderecoId;
        NomeEmpresaPretendido = Limpar(nomeEmpresaPretendido);
        NomeFantasiaPretendido = Limpar(nomeFantasiaPretendido);
        CadastroIptu = Limpar(cadastroIptu);
        NomeEdificacao = Limpar(nomeEdificacao);
        ProprietarioImovelNome = Limpar(proprietarioImovelNome);
        ProprietarioImovelCpfCnpj = Limpar(proprietarioImovelCpfCnpj);
        AreaTotalEdificacaoM2 = areaTotalEdificacaoM2;
        AreaOcupadaEmpresaM2 = areaOcupadaEmpresaM2;
        CapitalSocialPrevisto = capitalSocialPrevisto;
        EmailEmpresaPretendido = Limpar(emailEmpresaPretendido);
        TelefoneEmpresaPretendido = Limpar(telefoneEmpresaPretendido);
        NumeroFuncionariosPrevisto = numeroFuncionariosPrevisto;
        NumeroProLaboresPrevisto = numeroProLaboresPrevisto;
        LotacaoMaximaPublico = lotacaoMaximaPublico;
        HorarioFuncionamento = Limpar(horarioFuncionamento);
        DiasTrabalhadosMes = diasTrabalhadosMes;
        SituacaoImovel = Limpar(situacaoImovel)?.ToUpperInvariant();
        NumeroMaquinas = numeroMaquinas;
        TipoMaquinas = Limpar(tipoMaquinas);
        EnquadramentoPretendido = Limpar(enquadramentoPretendido);
        Observacoes = Limpar(observacoes);
        TipoEmpresaPretendido = Limpar(tipoEmpresaPretendido);
        RegimeTributarioPretendido = Limpar(regimeTributarioPretendido);
        AtualizadoEm = DateTime.UtcNow;
    }

    public void SubstituirAtividades(IEnumerable<FormularioAberturaAtividade> atividades)
    {
        _atividades.Clear();
        _atividades.AddRange(atividades);
    }

    private static string? Limpar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
