using Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.EnviarDocumento;
using Officio.ViewModels.Documentos;

namespace Officio.WebApi.Conversores;

public interface IDocumentoViewModelConversor
{
    EnviarDocumentoComando ParaComando(
        EnviarDocumentoViewModel viewModel,
        Stream arquivo,
        string nomeArquivo);
}
