using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class AdmissaoServico(
    IAdmissaoRepositorio repositorio,
    IClienteRepositorio clienteRepositorio,
    IEmpresaAtual empresaAtual,
    IPublicadorAtualizacaoTempoReal publicadorAtualizacaoTempoReal) : IAdmissaoServico
{
    public async Task<Resultado<Guid>> CriarManualAsync(
        CriarAdmissaoManualRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cliente = await clienteRepositorio.ObterPorIdAsync(requisicao.ClienteId, ct);
        if (cliente is null || !cliente.Ativo)
            return Resultado<Guid>.CriarFalha("Cliente ativo nao encontrado.");
        if (cliente.EmpresaId == Guid.Empty)
            return Resultado<Guid>.CriarFalha("Cliente nao possui empresa vinculada.");

        var validacao = ValidarDados(requisicao.Dados, exigirConclusao: false, exigirDadosBasicos: true);
        if (validacao is not null)
            return Resultado<Guid>.CriarFalha(validacao);

        var status = await ObterStatusAsync(StatusAdmissaoCodigos.Rascunho, ct);
        Admissao admissao;
        try
        {
            admissao = Admissao.Criar(
                empresaAtual.TenantId,
                cliente.EmpresaId,
                status.Id,
                requisicao.Dados.NomeFuncionario);
            AplicarDados(admissao, requisicao.Dados);
            var configuracoesDocumento = await repositorio.ObterOuCriarConfiguracoesDocumentoPadraoAsync(ct);
            admissao.AdicionarDocumentos(configuracoesDocumento.Select(configuracao =>
                AdmissaoDocumento.Criar(
                    admissao.Id,
                    configuracao.TipoDocumentoId,
                    configuracao.Obrigatorio,
                    configuracao.Ordem,
                    configuracao.RotuloExibicao)));
        }
        catch (ArgumentException exception)
        {
            return Resultado<Guid>.CriarFalha(exception.Message);
        }

        await repositorio.AdicionarAsync(admissao, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(admissao, ct);
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

        var validacao = ValidarDados(requisicao.Dados, exigirConclusao: false, exigirDadosBasicos: true);
        if (validacao is not null)
            return Resultado<Guid>.CriarFalha(validacao);

        var identificacaoAlterada = IdentificacaoAlterada(admissao, requisicao.Dados);

        try
        {
            AplicarDados(admissao, requisicao.Dados);
        }
        catch (ArgumentException exception)
        {
            return Resultado<Guid>.CriarFalha(exception.Message);
        }

        if (identificacaoAlterada && admissao.PreenchimentoFuncionarioConcluidoEm.HasValue)
        {
            admissao.MarcarDocumentosParaRevisao();
            var statusRevisao = await ObterStatusAsync(StatusAdmissaoCodigos.AguardandoValidacao, ct);
            admissao.AlterarStatus(statusRevisao.Id);
        }

        await repositorio.AdicionarDependentesAsync(admissao.Dependentes, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(admissao, ct);
        return Resultado<Guid>.CriarSucesso(admissao.Id);
    }

    public async Task<Resultado<List<AdmissaoResumoDto>>> ListarAsync(
        Guid? clienteId,
        string? status,
        string? busca,
        Guid? empresaId = null,
        CancellationToken ct = default)
    {
        var empresaIdFiltro = empresaId;
        if (!empresaIdFiltro.HasValue && clienteId.HasValue)
        {
            var cliente = await clienteRepositorio.ObterPorIdAsync(clienteId.Value, ct);
            if (cliente is null)
                return Resultado<List<AdmissaoResumoDto>>.CriarFalha("Cliente nao encontrado.");
            empresaIdFiltro = cliente.EmpresaId;
            if (empresaIdFiltro is null)
                return Resultado<List<AdmissaoResumoDto>>.CriarSucesso([]);
        }

        var admissoes = await repositorio.ListarAsync(empresaIdFiltro, status, busca, ct);
        return Resultado<List<AdmissaoResumoDto>>.CriarSucesso(admissoes.Select(MapearResumo).ToList());
    }

    public async Task<Resultado<List<EmpresaAdmissoesResumoDto>>> ListarEmpresasComAdmissoesAsync(
        string? busca,
        CancellationToken ct = default)
    {
        var empresas = await repositorio.ListarEmpresasComAdmissoesAsync(busca, ct);
        return Resultado<List<EmpresaAdmissoesResumoDto>>.CriarSucesso(empresas);
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

    public async Task<Resultado<bool>> ConcluirRevisaoDocumentalAsync(
        Guid admissaoId,
        CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<bool>.CriarFalha("Admissao nao encontrada.");
        if (admissao.StatusAdmissao.Codigo != StatusAdmissaoCodigos.AguardandoValidacao)
            return Resultado<bool>.CriarFalha("A admissao nao esta aguardando revisao documental da contabilidade.");
        if (!admissao.PreenchimentoFuncionarioConcluidoEm.HasValue)
            return Resultado<bool>.CriarFalha("O admitido ainda nao concluiu o preenchimento da ficha.");

        var erroDocumentos = AdmissaoDocumentoRegras.ValidarDocumentosParaConclusao(admissao.Documentos);
        if (erroDocumentos is not null)
            return Resultado<bool>.CriarFalha(erroDocumentos);

        var empregadorConcluiu = admissao.PreenchimentoEmpresaConcluidoEm.HasValue;
        var status = await repositorio.ObterOuCriarStatusAsync(
            empregadorConcluiu ? StatusAdmissaoCodigos.LiberadoParaRevisaoFinal : StatusAdmissaoCodigos.AguardandoEmpregador,
            empregadorConcluiu ? "Liberado para revisão final" : "Aguardando empregador",
            empregadorConcluiu ? 6 : 5,
            ct);
        admissao.AlterarStatus(status.Id);
        await repositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(admissao, ct);
        return Resultado<bool>.CriarSucesso(true);
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
        if (admissao.StatusAdmissao.Codigo is not (StatusAdmissaoCodigos.LiberadoParaRevisaoFinal or StatusAdmissaoCodigos.LiberadoPeloEmpregador) ||
            !admissao.PreenchimentoFuncionarioConcluidoEm.HasValue ||
            !admissao.PreenchimentoEmpresaConcluidoEm.HasValue)
            return Resultado<bool>.CriarFalha("A admissao so pode ser concluida apos o preenchimento do admitido, a validacao documental e a liberacao pelo empregador.");

        var dados = ExtrairDados(admissao);
        var validacao = ValidarDados(dados, exigirConclusao: true);
        if (validacao is not null)
            return Resultado<bool>.CriarFalha(validacao);
        var erroDocumentos = AdmissaoDocumentoRegras.ValidarDocumentosParaConclusao(admissao.Documentos);
        if (erroDocumentos is not null)
            return Resultado<bool>.CriarFalha(erroDocumentos);

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
        await PublicarAtualizacaoAsync(admissao, ct);
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
        await PublicarAtualizacaoAsync(admissao, ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private Task PublicarAtualizacaoAsync(Admissao admissao, CancellationToken ct)
        => publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(
                admissao.TenantId,
                EntidadesAtualizacaoTempoReal.Admissao,
                admissao.Id),
            ct);

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

    private static string? ValidarDados(
        DadosAdmissaoRequisicao dados,
        bool exigirConclusao,
        bool exigirDadosBasicos = false)
    {
        if (string.IsNullOrWhiteSpace(dados.NomeFuncionario))
            return "Nome do funcionario e obrigatorio.";
        var cpf = Comum.DocumentoFiscal.SomenteDigitos(dados.Cpf);
        if (exigirDadosBasicos && cpf is null)
            return "CPF do funcionario e obrigatorio para criar a admissao.";
        if (cpf is not null && cpf.Length != 11)
            return "CPF do funcionario deve conter 11 digitos.";
        if (exigirDadosBasicos && string.IsNullOrWhiteSpace(dados.Rg))
            return "RG do funcionario e obrigatorio para criar a admissao.";
        if (exigirDadosBasicos && dados.DataNascimento is null)
            return "Data de nascimento e obrigatoria para criar a admissao.";
        if (exigirDadosBasicos && dados.Sexo is not ("Masculino" or "Feminino"))
            return "Selecione Masculino ou Feminino para o sexo do funcionario.";
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
            StatusAdmissaoCodigos.AguardandoValidacao => repositorio.ObterOuCriarStatusAsync(codigo, "Aguardando validacao", 3, ct),
            StatusAdmissaoCodigos.Concluida => repositorio.ObterOuCriarStatusAsync(codigo, "Concluida", 7, ct),
            StatusAdmissaoCodigos.Cancelada => repositorio.ObterOuCriarStatusAsync(codigo, "Cancelada", 8, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(codigo), codigo, null)
        };

    private static bool IdentificacaoAlterada(Admissao admissao, DadosAdmissaoRequisicao dados)
        => !string.Equals(admissao.NomeFuncionario.Trim(), dados.NomeFuncionario.Trim(), StringComparison.Ordinal)
            || !string.Equals(Comum.DocumentoFiscal.SomenteDigitos(admissao.Cpf), Comum.DocumentoFiscal.SomenteDigitos(dados.Cpf), StringComparison.Ordinal)
            || !string.Equals(admissao.Rg?.Trim(), dados.Rg?.Trim(), StringComparison.OrdinalIgnoreCase)
            || admissao.DataNascimento != dados.DataNascimento
            || !string.Equals(admissao.Sexo?.Trim(), dados.Sexo?.Trim(), StringComparison.OrdinalIgnoreCase);

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
            admissao.Funcao, admissao.Salario, admissao.CriadoEm,
            admissao.PreenchimentoFuncionarioConcluidoEm.HasValue,
            admissao.PreenchimentoEmpresaConcluidoEm.HasValue);

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
