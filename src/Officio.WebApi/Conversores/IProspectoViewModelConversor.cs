using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;
using Officio.ViewModels.Prospectos;

namespace Officio.WebApi.Conversores;

public interface IProspectoViewModelConversor
{
    CriarProspectoComando ParaComando(CriarProspectoViewModel viewModel);

    ObterProspectosConsulta ParaConsulta(
        int pagina,
        int tamanhoPagina,
        string? status,
        string? busca,
        Guid? responsavelId);
}
