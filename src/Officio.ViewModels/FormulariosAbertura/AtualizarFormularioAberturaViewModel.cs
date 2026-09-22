namespace Officio.ViewModels.FormulariosAbertura;

public class AtualizarFormularioAberturaViewModel
{
    public Guid? PessoaResponsavelId { get; set; }
    public Guid? EnderecoId { get; set; }
    public string? NomeEmpresaPretendido { get; set; }
    public string? NomeFantasiaPretendido { get; set; }
    public string? CadastroIptu { get; set; }
    public string? NomeEdificacao { get; set; }
    public string? ProprietarioImovelNome { get; set; }
    public string? ProprietarioImovelCpfCnpj { get; set; }
    public decimal? AreaTotalEdificacaoM2 { get; set; }
    public decimal? AreaOcupadaEmpresaM2 { get; set; }
    public decimal? CapitalSocialPrevisto { get; set; }
    public string? EmailEmpresaPretendido { get; set; }
    public string? TelefoneEmpresaPretendido { get; set; }
    public int? NumeroFuncionariosPrevisto { get; set; }
    public int? NumeroProLaboresPrevisto { get; set; }
    public int? LotacaoMaximaPublico { get; set; }
    public string? HorarioFuncionamento { get; set; }
    public int? DiasTrabalhadosMes { get; set; }
    public string? SituacaoImovel { get; set; }
    public int? NumeroMaquinas { get; set; }
    public string? TipoMaquinas { get; set; }
    public string? EnquadramentoPretendido { get; set; }
    public string? Observacoes { get; set; }
    public string? TipoEmpresaPretendido { get; set; }
    public string? RegimeTributarioPretendido { get; set; }
    public List<AtividadeFormularioAberturaViewModel>? Atividades { get; set; }

}

public class AtividadeFormularioAberturaViewModel
{
    public string DescricaoAtividade { get; set; } = string.Empty;
    public string? Cnae { get; set; }
    public decimal? Percentual { get; set; }
    public bool? RealizaNoEndereco { get; set; }
    public int? Ordem { get; set; }
}
