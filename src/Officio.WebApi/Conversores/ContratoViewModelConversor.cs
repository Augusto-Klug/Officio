using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;
using Officio.ViewModels.Contratos;

namespace Officio.WebApi.Conversores;

internal static class ContratoViewModelConversor
{
    public static ConverterOrcamentoParaContratoComando ParaComando(
        this ConverterOrcamentoParaContratoViewModel viewModel,
        Guid orcamentoId)
        => new(
            orcamentoId,
            viewModel.DataInicio,
            viewModel.DataFim,
            viewModel.RenovacaoAutomatica,
            viewModel.ClausulasExtras);

    public static EnviarParaAssinaturaComando ParaComando(
        this EnviarAssinaturaViewModel viewModel,
        Guid contratoId)
        => new(
            contratoId,
            viewModel.Signatarios
                .Select(s => new SignatarioDto(s.Nome, s.Email, s.Cpf, s.Tipo))
                .ToList(),
            viewModel.MensagemPersonalizada);
}
