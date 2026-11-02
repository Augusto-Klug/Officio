namespace Officio.Dominio.Seguranca;

public static class PerfisAcesso
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string Admin = "Admin";
    public const string Operacional = "Operacional";
    public const string Consulta = "Consulta";
    public const string Cliente = "Cliente";

    public const string PerfisLeituraTenantCsv = Admin + "," + Operacional + "," + Consulta;
    public const string PerfisOperacaoTenantCsv = Admin + "," + Operacional;

    public static readonly IReadOnlySet<string> PerfisTenant = new HashSet<string>(
        [Admin, Operacional, Consulta, Cliente],
        StringComparer.OrdinalIgnoreCase);

    public static string? NormalizarPerfilTenant(string perfil)
        => PerfisTenant.FirstOrDefault(item => item.Equals(perfil?.Trim(), StringComparison.OrdinalIgnoreCase));
}
