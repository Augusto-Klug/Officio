using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;
using Officio.ViewModels.Contratos;

namespace Officio.WebApi.Conversores;

public interface IContratoViewModelConversor
{
    EnviarParaAssinaturaComando ParaComando(
        EnviarAssinaturaViewModel viewModel,
        Guid contratoId);
}
