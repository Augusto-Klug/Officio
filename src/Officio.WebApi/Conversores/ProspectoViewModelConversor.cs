using Officio.Dominio.Contratos;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;
using Officio.ViewModels.Enumeradores;
using Officio.ViewModels.Prospectos;

namespace Officio.WebApi.Conversores;

internal class ProspectoViewModelConversor : IProspectoViewModelConversor
{
    public CriarProspectoComando ParaComando(CriarProspectoViewModel viewModel)
        => new(
            new CriarProspectoRequisicao(
                viewModel.RazaoSocial,
                viewModel.Cnpj,
                Converter(viewModel.RegimeTributario),
                viewModel.Socios?
                    .Select(s => new CriarSocioRequisicao(s.Nome, s.Cpf, s.Participacao))
                    .ToList()));

    public ObterProspectosConsulta ParaConsulta(
        int pagina,
        int tamanhoPagina,
        string? status,
        string? busca,
        Guid? responsavelId)
        => new(
            new ObterProspectosFiltro(
                pagina,
                tamanhoPagina,
                status,
                busca,
                responsavelId));

    private static RegimeTributario? Converter(RegimeTributarioViewModel? regime)
        => regime is null ? null : regime.Value switch
        {
            RegimeTributarioViewModel.SimplesNacional => RegimeTributario.SimplesNacional,
            RegimeTributarioViewModel.LucroPresumido => RegimeTributario.LucroPresumido,
            RegimeTributarioViewModel.LucroReal => RegimeTributario.LucroReal,
            RegimeTributarioViewModel.MEI => RegimeTributario.MEI,
            _ => null
        };
}
