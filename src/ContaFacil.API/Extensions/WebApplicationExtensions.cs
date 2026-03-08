using ContaFacil.API.Middleware;

namespace ContaFacil.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseContaFacilMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseAuthentication();
        app.UseMiddleware<TenantMiddleware>();
        app.UseAuthorization();

        return app;
    }
}
