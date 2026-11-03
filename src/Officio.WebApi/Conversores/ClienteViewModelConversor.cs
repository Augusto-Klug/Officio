using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.AtualizarCliente;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.VincularCodigoSci;
using Officio.ViewModels.Clientes;
using Officio.ViewModels.Enumeradores;
using Officio.Dominio.Contratos;

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
            viewModel.Observacoes,
            viewModel.InscricaoEstadual,
            viewModel.InscricaoMunicipal,
            viewModel.TipoEmpresa,
            viewModel.MunicipioCodigoIbge,
            viewModel.Socios?.Select(s => new CriarSocioRequisicao(
                s.Nome, s.Cpf, s.Participacao, s.Id, s.Profissao, s.TipoRelacaoId,
                s.Endereco is null ? null : new EnderecoSocioRequisicao(
                    s.Endereco.Cep, s.Endereco.Logradouro, s.Endereco.Numero, s.Endereco.Complemento,
                    s.Endereco.Bairro, s.Endereco.Cidade, s.Endereco.Uf, s.Endereco.MunicipioCodigoIbge))).ToArray(),
            viewModel.ConfirmarCancelamentoContratos,
            viewModel.ContratosConfirmados,
            viewModel.Complemento);

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
            RegimeTributarioViewModel.SimplesNacionalHibrido => RegimeTributario.SimplesNacionalHibrido,
            _ => throw new ArgumentOutOfRangeException(nameof(regime), regime, null)
        };
}
