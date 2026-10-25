using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;
using Officio.ViewModels.Contratos;

namespace Officio.WebApi.Conversores;

internal class ContratoViewModelConversor : IContratoViewModelConversor
{
    public EnviarParaAssinaturaComando ParaComando(
        EnviarAssinaturaViewModel viewModel,
        Guid contratoId)
        => new(
            new EnviarAssinaturaRequisicao(
                contratoId,
                viewModel.Signatarios
                    .Select(s => new SignatarioAssinatura(s.Nome, s.Email, s.Cpf, s.Tipo))
                    .ToList(),
                viewModel.MensagemPersonalizada));
}

internal static class ContratoOrcamentoViewModelConversor
{
    public static ConverterOrcamentoParaContratoComando ParaComando(
        this ConverterOrcamentoParaContratoViewModel viewModel,
        Guid orcamentoId)
        => new(
            orcamentoId,
            viewModel.DataInicio,
            viewModel.DuracaoAnos,
            viewModel.CompetenciaInicialHonorarios,
            viewModel.ConfirmarSubstituicaoContratoAtivo,
            viewModel.ClausulasExtras,
            viewModel.RepresentanteSocioId);
}
