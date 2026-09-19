namespace Officio.Dominio.Seguranca;

public static class PerfisAcesso
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string Admin = "Admin";
    public const string Comercial = "Comercial";
    public const string Operacional = "Operacional";
    public const string Financeiro = "Financeiro";
    public const string Consulta = "Consulta";

    public static readonly IReadOnlySet<string> PerfisTenant = new HashSet<string>(
        [Admin, Comercial, Operacional, Financeiro, Consulta],
        StringComparer.OrdinalIgnoreCase);

    public static string? NormalizarPerfilTenant(string perfil)
        => PerfisTenant.FirstOrDefault(item => item.Equals(perfil?.Trim(), StringComparison.OrdinalIgnoreCase));
}
