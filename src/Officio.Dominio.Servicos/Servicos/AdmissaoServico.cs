using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class AdmissaoServico(
    IAdmissaoRepositorio repositorio,
    IClienteRepositorio clienteRepositorio,
    IEmpresaAtual empresaAtual) : IAdmissaoServico
{
    public async Task<Resultado<Guid>> CriarManualAsync(
        CriarAdmissaoManualRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cliente = await clienteRepositorio.ObterPorIdAsync(requisicao.ClienteId, ct);
        if (cliente is null || !cliente.Ativo)
            return Resultado<Guid>.CriarFalha("Cliente ativo nao encontrado.");
        if (cliente.EmpresaId is null)
            return Resultado<Guid>.CriarFalha("Cliente nao possui empresa vinculada.");

        var validacao = ValidarDados(requisicao.Dados, exigirConclusao: false);
        if (validacao is not null)
            return Resultado<Guid>.CriarFalha(validacao);

        var status = await ObterStatusAsync(StatusAdmissaoCodigos.Rascunho, ct);
        Admissao admissao;
        try
        {
            admissao = Admissao.Criar(
                empresaAtual.TenantId,
                cliente.EmpresaId.Value,
                status.Id,
                requisicao.Dados.NomeFuncionario);
            AplicarDados(admissao, requisicao.Dados);
        }
        catch (ArgumentException exception)
        {
            return Resultado<Guid>.CriarFalha(exception.Message);
        }

        await repositorio.AdicionarAsync(admissao, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<Guid>.CriarSucesso(admissao.Id);
    }

    public async Task<Resultado<Guid>> AtualizarAsync(
        AtualizarAdmissaoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterParaAlteracaoAsync(requisicao.AdmissaoId, ct);
        if (admissao is null)
            return Resultado<Guid>.CriarFalha("Admissao nao encontrada.");
        if (StatusAdmissaoCodigos.Terminal(admissao.StatusAdmissao.Codigo))
            return Resultado<Guid>.CriarFalha("Admissao concluida ou cancelada nao pode ser editada.");

        var validacao = ValidarDados(requisicao.Dados, exigirConclusao: false);
        if (validacao is not null)
            return Resultado<Guid>.CriarFalha(validacao);

        try
        {
            AplicarDados(admissao, requisicao.Dados);
        }
        catch (ArgumentException exception)
        {
            return Resultado<Guid>.CriarFalha(exception.Message);
        }

        await repositorio.AdicionarDependentesAsync(admissao.Dependentes, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<Guid>.CriarSucesso(admissao.Id);
    }

    public async Task<Resultado<List<AdmissaoResumoDto>>> ListarAsync(
        Guid? clienteId,
        string? status,
        string? busca,
        CancellationToken ct = default)
    {
        Guid? empresaId = null;
        if (clienteId.HasValue)
        {
            var cliente = await clienteRepositorio.ObterPorIdAsync(clienteId.Value, ct);
            if (cliente is null)
                return Resultado<List<AdmissaoResumoDto>>.CriarFalha("Cliente nao encontrado.");
            empresaId = cliente.EmpresaId;
            if (empresaId is null)
                return Resultado<List<AdmissaoResumoDto>>.CriarSucesso([]);
        }

        var admissoes = await repositorio.ListarAsync(empresaId, status, busca, ct);
        return Resultado<List<AdmissaoResumoDto>>.CriarSucesso(admissoes.Select(MapearResumo).ToList());
    }

    public async Task<Resultado<AdmissaoDetalheDto>> ObterPorIdAsync(
        Guid admissaoId,
        CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterDetalheAsync(admissaoId, ct);
        return admissao is null
            ? Resultado<AdmissaoDetalheDto>.CriarFalha("Admissao nao encontrada.")
            : Resultado<AdmissaoDetalheDto>.CriarSucesso(MapearDetalhe(admissao));
    }

    public async Task<Resultado<bool>> ConcluirAsync(Guid admissaoId, CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<bool>.CriarFalha("Admissao nao encontrada.");
        if (admissao.StatusAdmissao.Codigo == StatusAdmissaoCodigos.Cancelada)
            return Resultado<bool>.CriarFalha("Admissao cancelada nao pode ser concluida.");

        var funcionarioExistente = await repositorio.ObterFuncionarioPorAdmissaoAsync(admissao.Id, ct);
        if (admissao.StatusAdmissao.Codigo == StatusAdmissaoCodigos.Concluida || funcionarioExistente is not null)
            return Resultado<bool>.CriarSucesso(true);

        var dados = ExtrairDados(admissao);
        var validacao = ValidarDados(dados, exigirConclusao: true);
        if (validacao is not null)
            return Resultado<bool>.CriarFalha(validacao);

        var status = await ObterStatusAsync(StatusAdmissaoCodigos.Concluida, ct);
        var funcionario = EmpresaFuncionario.Criar(
            admissao.EmpresaId,
            admissao.Id,
            admissao.NomeFuncionario,
            admissao.DataAdmissaoPrevista!.Value,
            admissao.Cpf,
            admissao.PessoaId,
            admissao.Funcao,
            admissao.Salario);

        admissao.AlterarStatus(status.Id);
        admissao.Empresa.IncrementarQuantidadeFuncionarios();
        await repositorio.AdicionarFuncionarioAsync(funcionario, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> CancelarAsync(Guid admissaoId, CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<bool>.CriarFalha("Admissao nao encontrada.");
        if (admissao.StatusAdmissao.Codigo == StatusAdmissaoCodigos.Concluida)
            return Resultado<bool>.CriarFalha("Admissao concluida nao pode ser cancelada.");
        if (admissao.StatusAdmissao.Codigo == StatusAdmissaoCodigos.Cancelada)
            return Resultado<bool>.CriarSucesso(true);

        var status = await ObterStatusAsync(StatusAdmissaoCodigos.Cancelada, ct);
        admissao.AlterarStatus(status.Id);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private static void AplicarDados(Admissao admissao, DadosAdmissaoRequisicao dados)
    {
        admissao.AtualizarDados(
            dados.NomeFuncionario, dados.Cpf, dados.Rg, dados.DataNascimento, dados.Sexo,
            dados.EstadoCivil, dados.Escolaridade, dados.Telefone, dados.Email, dados.Pis,
            dados.Banco, dados.Agencia, dados.Conta, dados.TipoConta, dados.DataAdmissaoPrevista,
            dados.Salario, dados.Comissao, dados.Funcao, dados.ContratoExperienciaDias,
            dados.UtilizaValeTransporte, dados.HorarioTrabalho, dados.Observacoes);

        admissao.SubstituirDependentes(dados.Dependentes.Select(dependente =>
            AdmissaoDependente.Criar(
                admissao.Id, dependente.Nome, dependente.Cpf, dependente.DataNascimento,
                dependente.Parentesco, dependente.DependenteIr, dependente.SalarioFamilia)));
    }

    private static string? ValidarDados(DadosAdmissaoRequisicao dados, bool exigirConclusao)
    {
        if (string.IsNullOrWhiteSpace(dados.NomeFuncionario))
            return "Nome do funcionario e obrigatorio.";
        var cpf = Comum.DocumentoFiscal.SomenteDigitos(dados.Cpf);
        if (cpf is not null && cpf.Length != 11)
            return "CPF do funcionario deve conter 11 digitos.";
        if (dados.Dependentes.Any(dependente => string.IsNullOrWhiteSpace(dependente.Nome)))
            return "Nome do dependente e obrigatorio.";
        if (dados.Dependentes.Any(dependente =>
                Comum.DocumentoFiscal.SomenteDigitos(dependente.Cpf) is { Length: not 11 }))
            return "CPF do dependente deve conter 11 digitos.";
        if (!exigirConclusao)
            return null;
        if (cpf is null)
            return "CPF do funcionario e obrigatorio para concluir a admissao.";
        if (dados.DataAdmissaoPrevista is null)
            return "Data de admissao e obrigatoria para concluir a admissao.";
        if (string.IsNullOrWhiteSpace(dados.Funcao))
            return "Funcao do funcionario e obrigatoria para concluir a admissao.";
        return null;
    }

    private Task<StatusAdmissao> ObterStatusAsync(string codigo, CancellationToken ct)
        => codigo switch
        {
            StatusAdmissaoCodigos.Rascunho => repositorio.ObterOuCriarStatusAsync(codigo, "Rascunho", 1, ct),
            StatusAdmissaoCodigos.Concluida => repositorio.ObterOuCriarStatusAsync(codigo, "Concluida", 5, ct),
            StatusAdmissaoCodigos.Cancelada => repositorio.ObterOuCriarStatusAsync(codigo, "Cancelada", 6, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(codigo), codigo, null)
        };

    private static DadosAdmissaoRequisicao ExtrairDados(Admissao admissao)
        => new(
            admissao.NomeFuncionario, admissao.Cpf, admissao.Rg, admissao.DataNascimento,
            admissao.Sexo, admissao.EstadoCivil, admissao.Escolaridade, admissao.Telefone,
            admissao.Email, admissao.Pis, admissao.Banco, admissao.Agencia, admissao.Conta,
            admissao.TipoConta, admissao.DataAdmissaoPrevista, admissao.Salario, admissao.Comissao,
            admissao.Funcao, admissao.ContratoExperienciaDias, admissao.UtilizaValeTransporte,
            admissao.HorarioTrabalho, admissao.Observacoes,
            admissao.Dependentes.Select(item => new DependenteAdmissaoRequisicao(
                item.Nome, item.Cpf, item.DataNascimento, item.Parentesco,
                item.DependenteIr, item.SalarioFamilia)).ToList());

    private static AdmissaoResumoDto MapearResumo(Admissao admissao)
        => new(
            admissao.Id, admissao.EmpresaId, admissao.Empresa.RazaoSocial,
            admissao.NomeFuncionario, admissao.Cpf, admissao.StatusAdmissao.Codigo,
            admissao.StatusAdmissao.Descricao, admissao.DataAdmissaoPrevista,
            admissao.Funcao, admissao.Salario, admissao.CriadoEm);

    private static AdmissaoDetalheDto MapearDetalhe(Admissao admissao)
        => new(
            admissao.Id, admissao.EmpresaId, admissao.Empresa.RazaoSocial,
            admissao.NomeFuncionario, admissao.Cpf, admissao.Rg, admissao.DataNascimento,
            admissao.Sexo, admissao.EstadoCivil, admissao.Escolaridade, admissao.Telefone,
            admissao.Email, admissao.Pis, admissao.Banco, admissao.Agencia, admissao.Conta,
            admissao.TipoConta, admissao.DataAdmissaoPrevista, admissao.Salario, admissao.Comissao,
            admissao.Funcao, admissao.ContratoExperienciaDias, admissao.UtilizaValeTransporte,
            admissao.HorarioTrabalho, admissao.Observacoes, admissao.StatusAdmissao.Codigo,
            admissao.StatusAdmissao.Descricao, admissao.CriadoEm, admissao.AtualizadoEm,
            admissao.Dependentes.Select(item => new AdmissaoDependenteDto(
                item.Id, item.Nome, item.Cpf, item.DataNascimento, item.Parentesco,
                item.DependenteIr, item.SalarioFamilia)).ToList());
}
