using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.EnviarDocumento;
using Officio.ViewModels.Documentos;
using Officio.ViewModels.Enumeradores;

namespace Officio.WebApi.Conversores;

internal static class DocumentoViewModelConversor
{
    public static EnviarDocumentoComando ParaComando(
        this EnviarDocumentoViewModel viewModel,
        Stream arquivo,
        string nomeArquivo)
        => new(
            arquivo,
            nomeArquivo,
            Converter(viewModel.Tipo),
            viewModel.ClienteId,
            viewModel.ProspectoId,
            viewModel.SocioId,
            viewModel.Observacoes);

    private static TipoDocumento Converter(TipoDocumentoViewModel tipo)
        => tipo switch
        {
            TipoDocumentoViewModel.CartaoCnpj => TipoDocumento.CartaoCnpj,
            TipoDocumentoViewModel.ContratoSocial => TipoDocumento.ContratoSocial,
            TipoDocumentoViewModel.RgSocio => TipoDocumento.RgSocio,
            TipoDocumentoViewModel.CpfSocio => TipoDocumento.CpfSocio,
            TipoDocumentoViewModel.CnhSocio => TipoDocumento.CnhSocio,
            _ => TipoDocumento.CartaoCnpj
        };
}
