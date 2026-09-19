using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AtualizarAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CriarAdmissaoManual;
using Officio.ViewModels.Admissoes;

namespace Officio.WebApi.Conversores;

internal static class AdmissaoViewModelConversor
{
    public static CriarAdmissaoManualComando ParaComando(this CriarAdmissaoManualViewModel viewModel)
        => new(viewModel.ClienteId, Converter(viewModel.Dados));

    public static AtualizarAdmissaoComando ParaComando(this AtualizarAdmissaoViewModel viewModel, Guid id)
        => new(id, Converter(viewModel.Dados));

    private static DadosAdmissaoRequisicao Converter(DadosAdmissaoViewModel dados)
        => new(
            dados.NomeFuncionario,
            dados.Cpf,
            dados.Rg,
            dados.DataNascimento,
            dados.Sexo,
            dados.EstadoCivil,
            dados.Escolaridade,
            dados.Telefone,
            dados.Email,
            dados.Pis,
            dados.Banco,
            dados.Agencia,
            dados.Conta,
            dados.TipoConta,
            dados.DataAdmissaoPrevista,
            dados.Salario,
            dados.Comissao,
            dados.Funcao,
            dados.ContratoExperienciaDias,
            dados.UtilizaValeTransporte,
            dados.HorarioTrabalho,
            dados.Observacoes,
            (dados.Dependentes ?? []).Select(dependente => new DependenteAdmissaoRequisicao(
                dependente.Nome,
                dependente.Cpf,
                dependente.DataNascimento,
                dependente.Parentesco,
                dependente.DependenteIr,
                dependente.SalarioFamilia)).ToList());
}
