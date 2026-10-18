using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Admissao : EntidadeAuditavel
{
    private readonly List<AdmissaoDocumento> _documentos = [];
    private readonly List<AdmissaoDependente> _dependentes = [];

    public Guid EmpresaId { get; private set; }
    public Guid? PessoaId { get; private set; }
    public Guid StatusAdmissaoId { get; private set; }
    public string NomeFuncionario { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public string? Rg { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public string? Sexo { get; private set; }
    public string? EstadoCivil { get; private set; }
    public string? Escolaridade { get; private set; }
    public Guid? EnderecoId { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Pis { get; private set; }
    public string? Banco { get; private set; }
    public string? Agencia { get; private set; }
    public string? Conta { get; private set; }
    public string? TipoConta { get; private set; }
    public DateOnly? DataAdmissaoPrevista { get; private set; }
    public decimal? Salario { get; private set; }
    public string? Comissao { get; private set; }
    public string? Funcao { get; private set; }
    public int? ContratoExperienciaDias { get; private set; }
    public bool? UtilizaValeTransporte { get; private set; }
    public string? HorarioTrabalho { get; private set; }
    public string? TokenAcessoHash { get; private set; }
    public DateTime? TokenExpiraEm { get; private set; }
    public string? TokenEmpresaAcessoHash { get; private set; }
    public DateTime? TokenEmpresaExpiraEm { get; private set; }
    public DateTime? PreenchimentoFuncionarioConcluidoEm { get; private set; }
    public DateTime? PreenchimentoEmpresaConcluidoEm { get; private set; }
    public string? Observacoes { get; private set; }
    public Empresa Empresa { get; private set; } = default!;
    public Pessoa? Pessoa { get; private set; }
    public StatusAdmissao StatusAdmissao { get; private set; } = default!;
    public Endereco? Endereco { get; private set; }
    public IReadOnlyCollection<AdmissaoDocumento> Documentos => _documentos.AsReadOnly();
    public IReadOnlyCollection<AdmissaoDependente> Dependentes => _dependentes.AsReadOnly();

    private Admissao() { }

    public static Admissao Criar(Guid tenantId, Guid empresaId, Guid statusAdmissaoId, string nomeFuncionario)
    {
        if (string.IsNullOrWhiteSpace(nomeFuncionario))
            throw new ArgumentException("Nome do funcionario e obrigatorio.", nameof(nomeFuncionario));

        return new()
        {
            TenantId = tenantId,
            EmpresaId = empresaId,
            StatusAdmissaoId = statusAdmissaoId,
            NomeFuncionario = nomeFuncionario.Trim()
        };
    }

    public void AtualizarDados(
        string nomeFuncionario,
        string? cpf,
        string? rg,
        DateOnly? dataNascimento,
        string? sexo,
        string? estadoCivil,
        string? escolaridade,
        string? telefone,
        string? email,
        string? pis,
        string? banco,
        string? agencia,
        string? conta,
        string? tipoConta,
        DateOnly? dataAdmissaoPrevista,
        decimal? salario,
        string? comissao,
        string? funcao,
        int? contratoExperienciaDias,
        bool? utilizaValeTransporte,
        string? horarioTrabalho,
        string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(nomeFuncionario))
            throw new ArgumentException("Nome do funcionario e obrigatorio.", nameof(nomeFuncionario));
        if (salario < 0)
            throw new ArgumentException("Salario nao pode ser negativo.", nameof(salario));
        if (contratoExperienciaDias < 0)
            throw new ArgumentException("Contrato de experiencia nao pode ter duracao negativa.", nameof(contratoExperienciaDias));

        NomeFuncionario = nomeFuncionario.Trim();
        Cpf = DocumentoFiscal.SomenteDigitos(cpf);
        Rg = Normalizar(rg);
        DataNascimento = dataNascimento;
        Sexo = Normalizar(sexo);
        EstadoCivil = Normalizar(estadoCivil);
        Escolaridade = Normalizar(escolaridade);
        Telefone = Normalizar(telefone);
        Email = Normalizar(email)?.ToLowerInvariant();
        Pis = DocumentoFiscal.SomenteDigitos(pis);
        Banco = Normalizar(banco);
        Agencia = Normalizar(agencia);
        Conta = Normalizar(conta);
        TipoConta = Normalizar(tipoConta);
        DataAdmissaoPrevista = dataAdmissaoPrevista;
        Salario = salario;
        Comissao = Normalizar(comissao);
        Funcao = Normalizar(funcao);
        ContratoExperienciaDias = contratoExperienciaDias;
        UtilizaValeTransporte = utilizaValeTransporte;
        HorarioTrabalho = Normalizar(horarioTrabalho);
        Observacoes = Normalizar(observacoes);
    }

    public void SubstituirDependentes(IEnumerable<AdmissaoDependente> dependentes)
    {
        _dependentes.Clear();
        _dependentes.AddRange(dependentes);
    }

    public void AdicionarDocumentos(IEnumerable<AdmissaoDocumento> documentos)
        => _documentos.AddRange(documentos);

    public void AlterarStatus(Guid statusAdmissaoId)
        => StatusAdmissaoId = statusAdmissaoId;

    public void GerarAcessoTemporario(string tokenHash, DateTime expiraEm)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Hash do token e obrigatorio.", nameof(tokenHash));
        if (expiraEm <= DateTime.UtcNow)
            throw new ArgumentException("A expiracao do token deve estar no futuro.", nameof(expiraEm));

        TokenAcessoHash = tokenHash;
        TokenExpiraEm = expiraEm;
        PreenchimentoFuncionarioConcluidoEm = null;
        TokenEmpresaAcessoHash = null;
        TokenEmpresaExpiraEm = null;
        PreenchimentoEmpresaConcluidoEm = null;
    }

    public void RevogarAcessoTemporario()
    {
        TokenAcessoHash = null;
        TokenExpiraEm = null;
    }

    public void GerarAcessoEmpresa(string tokenHash, DateTime expiraEm)
    {
        if (!PreenchimentoFuncionarioConcluidoEm.HasValue)
            throw new InvalidOperationException("O admitido precisa concluir a ficha antes do acesso do empregador.");
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Hash do token e obrigatorio.", nameof(tokenHash));
        if (expiraEm <= DateTime.UtcNow)
            throw new ArgumentException("A expiracao do token deve estar no futuro.", nameof(expiraEm));

        TokenEmpresaAcessoHash = tokenHash;
        TokenEmpresaExpiraEm = expiraEm;
        PreenchimentoEmpresaConcluidoEm = null;
    }

    public void RevogarAcessoEmpresa()
    {
        TokenEmpresaAcessoHash = null;
        TokenEmpresaExpiraEm = null;
    }

    public void MarcarPreenchimentoFuncionarioConcluido()
        => PreenchimentoFuncionarioConcluidoEm ??= DateTime.UtcNow;

    public void MarcarPreenchimentoEmpresaConcluido()
        => PreenchimentoEmpresaConcluidoEm ??= DateTime.UtcNow;

    public void AtualizarDadosFuncionario(
        string nomeFuncionario,
        DateOnly? dataNascimento,
        string? sexo,
        string? estadoCivil,
        string? escolaridade,
        string? telefone,
        string? email)
    {
        if (string.IsNullOrWhiteSpace(nomeFuncionario))
            throw new ArgumentException("Nome do funcionario e obrigatorio.", nameof(nomeFuncionario));
        if (dataNascimento is null || dataNascimento >= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Informe uma data de nascimento valida.", nameof(dataNascimento));
        if (sexo is not ("Masculino" or "Feminino"))
            throw new ArgumentException("Selecione Masculino ou Feminino para o sexo do funcionario.", nameof(sexo));

        NomeFuncionario = nomeFuncionario.Trim();
        DataNascimento = dataNascimento;
        Sexo = sexo;
        EstadoCivil = Normalizar(estadoCivil);
        Escolaridade = Normalizar(escolaridade);
        Telefone = Normalizar(telefone);
        Email = Normalizar(email)?.ToLowerInvariant();
    }

    public void AtualizarDadosEmpresa(
        string? pis,
        DateOnly? dataAdmissaoPrevista,
        decimal? salario,
        string? comissao,
        string? funcao,
        int? contratoExperienciaDias,
        bool? utilizaValeTransporte,
        string? horarioTrabalho,
        string? banco,
        string? agencia,
        string? conta,
        string? tipoConta)
    {
        if (salario < 0)
            throw new ArgumentException("Salario nao pode ser negativo.", nameof(salario));
        if (contratoExperienciaDias < 0)
            throw new ArgumentException("Contrato de experiencia nao pode ter duracao negativa.", nameof(contratoExperienciaDias));

        Pis = DocumentoFiscal.SomenteDigitos(pis);
        DataAdmissaoPrevista = dataAdmissaoPrevista;
        Salario = salario;
        Comissao = Normalizar(comissao);
        Funcao = Normalizar(funcao);
        ContratoExperienciaDias = contratoExperienciaDias;
        UtilizaValeTransporte = utilizaValeTransporte;
        HorarioTrabalho = Normalizar(horarioTrabalho);
        Banco = Normalizar(banco);
        Agencia = Normalizar(agencia);
        Conta = Normalizar(conta);
        TipoConta = Normalizar(tipoConta);
    }

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
