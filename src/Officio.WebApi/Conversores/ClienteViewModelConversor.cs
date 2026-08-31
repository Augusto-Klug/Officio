using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.AtualizarCliente;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.VincularCodigoSci;
using Officio.ViewModels.Clientes;
using Officio.ViewModels.Enumeradores;

namespace Officio.WebApi.Conversores;

internal static class ClienteViewModelConversor
{
    public static AtualizarClienteComando ParaComando(this AtualizarClienteViewModel viewModel, Guid id)
        => new(
            id,
            viewModel.RazaoSocial,
            viewModel.NomeFantasia,
            viewModel.Cnpj,
            viewModel.Cpf,
            Converter(viewModel.RegimeTributario),
            viewModel.Email,
            viewModel.Telefone,
            viewModel.Whatsapp,
            viewModel.Cep,
            viewModel.Logradouro,
            viewModel.Numero,
            viewModel.Bairro,
            viewModel.Cidade,
            viewModel.Uf,
            viewModel.Observacoes);

    public static VincularCodigoSciComando ParaComando(this VincularCodigoSciViewModel viewModel, Guid id)
        => new(id, viewModel.SciCodigo);

    private static RegimeTributario? Converter(RegimeTributarioViewModel? regime)
        => regime switch
        {
            null => null,
            RegimeTributarioViewModel.SimplesNacional => RegimeTributario.SimplesNacional,
            RegimeTributarioViewModel.LucroPresumido => RegimeTributario.LucroPresumido,
            RegimeTributarioViewModel.LucroReal => RegimeTributario.LucroReal,
            RegimeTributarioViewModel.MEI => RegimeTributario.MEI,
            _ => throw new ArgumentOutOfRangeException(nameof(regime), regime, null)
        };
}
