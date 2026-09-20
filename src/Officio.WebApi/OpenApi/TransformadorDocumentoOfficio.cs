using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Officio.WebApi.OpenApi;

internal sealed class TransformadorDocumentoOfficio : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Officio API",
            Version = "v1",
            Description = "API para gestao contabil, comercial, contratual e operacional do Officio."
        };

        return Task.CompletedTask;
    }
}
