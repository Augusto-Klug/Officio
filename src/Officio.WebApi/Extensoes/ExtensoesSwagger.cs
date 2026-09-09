namespace Officio.WebApi.Extensoes;

public static class ExtensoesSwagger
{
    public static WebApplication UsarSwaggerOfficio(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
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
              <div style="padding: 12px 16px; font-family: sans-serif; border-bottom: 1px solid #ddd;">
                <button onclick="setJwt()">Definir JWT</button>
                <button onclick="clearJwt()">Limpar JWT</button>
                <span style="margin-left: 12px;">O token fica apenas no localStorage deste navegador.</span>
              </div>
              <div id="swagger-ui"></div>
              <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
              <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-standalone-preset.js"></script>
              <script>
                function setJwt() {
                  const token = prompt("Cole o JWT sem o prefixo Bearer:");
                  if (token) localStorage.setItem("officio.jwt", token.trim());
                }

                function clearJwt() {
                  localStorage.removeItem("officio.jwt");
                }

                SwaggerUIBundle({
                  url: "/openapi/v1.json",
                  dom_id: "#swagger-ui",
                  presets: [
                    SwaggerUIBundle.presets.apis,
                    SwaggerUIStandalonePreset
                  ],
                  layout: "StandaloneLayout",
                  requestInterceptor: function(request) {
                    const token = localStorage.getItem("officio.jwt");
                    if (token) request.headers.Authorization = "Bearer " + token;
                    return request;
                  }
                });
              </script>
            </body>
            </html>
            """,
            "text/html"));

        return app;
    }
}
