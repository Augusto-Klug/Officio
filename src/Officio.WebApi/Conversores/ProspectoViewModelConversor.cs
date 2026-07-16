using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;
using Officio.ViewModels.Enumeradores;
using Officio.ViewModels.Prospectos;

namespace Officio.WebApi.Conversores;

internal static class ProspectoViewModelConversor
{
    public static CriarProspectoComando ParaComando(this CriarProspectoViewModel viewModel)
        => new(
            viewModel.RazaoSocial,
            viewModel.NomeFantasia,
            viewModel.Cnpj,
            viewModel.Cpf,
            Converter(viewModel.RegimeTributario),
            viewModel.PorteEmpresa,
            viewModel.CnaePrincipal,
            viewModel.DataAbertura,
            viewModel.Email,
            viewModel.Telefone,
            viewModel.Whatsapp,
            viewModel.NomeContato,
            viewModel.Cep,
            viewModel.Logradouro,
            viewModel.Numero,
            viewModel.Bairro,
            viewModel.Cidade,
            viewModel.Uf,
            viewModel.Origem,
            viewModel.ResponsavelId,
            viewModel.Socios?
                .Select(s => new CriarSocioDto(s.Nome, s.Cpf, s.Participacao))
                .ToList());

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
