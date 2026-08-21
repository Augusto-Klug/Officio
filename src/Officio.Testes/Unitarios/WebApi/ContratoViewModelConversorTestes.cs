using Officio.ViewModels.Contratos;
using Officio.WebApi.Conversores;

namespace Officio.Testes.Unitarios.WebApi;

public class ContratoViewModelConversorTestes
{
    [Fact]
    public void ConverterOrcamentoParaContratoViewModel_ParaComando_MapeiaCampos()
    {
        var orcamentoId = Guid.NewGuid();
        var viewModel = new ConverterOrcamentoParaContratoViewModel
        {
            DataInicio = new DateTime(2026, 9, 1),
            DataFim = new DateTime(2027, 9, 1),
            RenovacaoAutomatica = true,
            ClausulasExtras = "Clausula"
        };

        var comando = viewModel.ParaComando(orcamentoId);

        Assert.Equal(orcamentoId, comando.OrcamentoId);
        Assert.Equal(viewModel.DataInicio, comando.DataInicio);
        Assert.Equal(viewModel.DataFim, comando.DataFim);
        Assert.True(comando.RenovacaoAutomatica);
        Assert.Equal("Clausula", comando.ClausulasExtras);
    }
}
