using Officio.ViewModels.Contratos;
using Officio.WebApi.Conversores;

namespace Officio.Testes.Unitarios.WebApi;

public class ContratoViewModelConversorTestes
{
    [Fact]
    public void ConverterOrcamentoParaContratoViewModel_DuracaoPadraoEhQuatroAnos()
    {
        var viewModel = new ConverterOrcamentoParaContratoViewModel();

        Assert.Equal(4, viewModel.DuracaoAnos);
    }

    [Fact]
    public void ConverterOrcamentoParaContratoViewModel_ParaComando_MapeiaCampos()
    {
        var orcamentoId = Guid.NewGuid();
        var viewModel = new ConverterOrcamentoParaContratoViewModel
        {
            DataInicio = new DateTime(2026, 9, 1),
            DuracaoAnos = 1,
            CompetenciaInicialHonorarios = new DateOnly(2026, 11, 1),
            ConfirmarSubstituicaoContratoAtivo = true,
            ClausulasExtras = "Clausula",
            RepresentanteSocioId = Guid.NewGuid()
        };

        var comando = viewModel.ParaComando(orcamentoId);

        Assert.Equal(orcamentoId, comando.OrcamentoId);
        Assert.Equal(viewModel.DataInicio, comando.DataInicio);
        Assert.Equal(viewModel.DuracaoAnos, comando.DuracaoAnos);
        Assert.Equal(viewModel.CompetenciaInicialHonorarios, comando.CompetenciaInicialHonorarios);
        Assert.True(comando.ConfirmarSubstituicaoContratoAtivo);
        Assert.Equal("Clausula", comando.ClausulasExtras);
        Assert.Equal(viewModel.RepresentanteSocioId, comando.RepresentanteSocioId);
    }
}
