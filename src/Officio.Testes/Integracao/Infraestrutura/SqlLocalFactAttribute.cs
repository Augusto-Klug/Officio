using Xunit;

namespace Officio.Testes.Integracao.Infraestrutura;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class SqlLocalFactAttribute : FactAttribute
{
    public SqlLocalFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OFFICIO_SQL_TEST_CONNECTION")))
            Skip = "Configure OFFICIO_SQL_TEST_CONNECTION para validar bloqueios no SQL Server local.";
    }
}
