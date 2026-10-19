using Xunit;

namespace Officio.Testes.Integracao.Infraestrutura;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class OcrRealFactAttribute : FactAttribute
{
    public OcrRealFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("OFFICIO_OCR_INTEGRACAO_REAL") != "1")
            Skip = "Defina OFFICIO_OCR_INTEGRACAO_REAL=1 para executar chamadas reais ao Azure e Azurite.";
    }
}
