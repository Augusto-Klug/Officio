namespace Officio.Dominio.Contratos;

public record AtualizarFormularioAberturaRequisicao(
    Guid ProspectoId,
    Guid? PessoaResponsavelId,
    Guid? EnderecoId,
    string? NomeEmpresaPretendido,
    string? NomeFantasiaPretendido,
    string? CadastroIptu,
    string? NomeEdificacao,
    string? ProprietarioImovelNome,
    string? ProprietarioImovelCpfCnpj,
    decimal? AreaTotalEdificacaoM2,
    decimal? AreaOcupadaEmpresaM2,
    decimal? CapitalSocialPrevisto,
    string? EmailEmpresaPretendido,
    string? TelefoneEmpresaPretendido,
    int? NumeroFuncionariosPrevisto,
    int? NumeroProLaboresPrevisto,
    int? LotacaoMaximaPublico,
    string? HorarioFuncionamento,
    int? DiasTrabalhadosMes,
    string? SituacaoImovel,
    int? NumeroMaquinas,
    string? TipoMaquinas,
    string? EnquadramentoPretendido,
    string? Observacoes,
    string? TipoEmpresaPretendido,
    string? RegimeTributarioPretendido,
    IReadOnlyCollection<AtividadeFormularioAberturaRequisicao> Atividades);

public record AtividadeFormularioAberturaRequisicao(
    string DescricaoAtividade,
    string? Cnae,
    decimal? Percentual,
    bool? RealizaNoEndereco,
    int? Ordem);
