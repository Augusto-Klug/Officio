namespace Officio.WebApi.Extensoes;

public static class ExtensoesSwagger
{
    public static IServiceCollection AdicionarOpenApiOfficio(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<OpenApi.TransformadorDocumentoOfficio>();
            options.AddDocumentTransformer<OpenApi.TransformadorAutenticacaoOpenApi>();
            options.AddOperationTransformer<OpenApi.TransformadorOperacaoAutorizacao>();
        });

        return services;
    }

    public static WebApplication UsarSwaggerOfficio(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
            return app;

        app.MapOpenApi();
        app.MapGet("/swagger", () => Results.Content(
            """
            <!doctype html>
            <html lang="pt-BR">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Officio API</title>
              <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css">
            </head>
            <body>
              <div id="swagger-ui"></div>
              <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
              <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-standalone-preset.js"></script>
              <script>
                SwaggerUIBundle({
                  url: "/openapi/v1.json",
                  dom_id: "#swagger-ui",
                  presets: [
                    SwaggerUIBundle.presets.apis,
                    SwaggerUIStandalonePreset
                  ],
                  layout: "StandaloneLayout",
                  persistAuthorization: true,
                  displayRequestDuration: true,
                  filter: true,
                  tryItOutEnabled: true
                });
              </script>
            </body>
            </html>
            """,
            "text/html"));

        return app;
    }
}
