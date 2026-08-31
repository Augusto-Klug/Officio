using Officio.WebApi.Intermediarios;

namespace Officio.WebApi.Extensoes;

public static class ExtensoesWebApplication
{
    public static WebApplication UsarMiddlewaresOfficio(this WebApplication app)
    {
        app.UseMiddleware<IntermediarioExcecao>();
        app.UseAuthentication();
        app.UseMiddleware<IntermediarioEmpresa>();
        app.UseAuthorization();

        return app;
    }
}




