using Officio.Dominio.Enumeradores;
using Officio.ViewModels.Enumeradores;
using Officio.ViewModels.Prospectos;
using Officio.WebApi.Conversores;

namespace Officio.Testes.Unitarios.WebApi;

public class ProspectoViewModelConversorTestes
{
    [Fact]
    public void ParaComando_MapeiaSimplesHibridoSemAlterarValorPersistido()
    {
        var viewModel = new CriarProspectoViewModel
        {
            RazaoSocial = "Empresa Exemplo",
            RegimeTributario = RegimeTributarioViewModel.SimplesNacionalHibrido
        };

        var comando = new ProspectoViewModelConversor().ParaComando(viewModel);

        Assert.Equal(4, (int)RegimeTributario.SimplesNacionalHibrido);
        Assert.Equal(RegimeTributario.SimplesNacionalHibrido, comando.Requisicao.RegimeTributario);
    }
}
