using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Officio.WebApi.OpenApi;

internal sealed class TransformadorOperacaoAutorizacao : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadados = context.Description.ActionDescriptor.EndpointMetadata;
        var permiteAnonimo = metadados.OfType<IAllowAnonymous>().Any();
        var autorizacoes = metadados.OfType<IAuthorizeData>().ToList();

        DefinirIdentificadorOperacao(operation, context);

        if (permiteAnonimo || autorizacoes.Count == 0)
            return Task.CompletedTask;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, context.Document)] = []
        });

        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Nao autenticado." });

        if (autorizacoes.Any(autorizacao => !string.IsNullOrWhiteSpace(autorizacao.Roles)))
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Acesso negado." });

        return Task.CompletedTask;
    }

    private static void DefinirIdentificadorOperacao(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context)
    {
        if (!string.IsNullOrWhiteSpace(operation.OperationId))
            return;

        var rotas = context.Description.ActionDescriptor.RouteValues;
        if (rotas.TryGetValue("controller", out var controller) &&
            rotas.TryGetValue("action", out var action))
            operation.OperationId = $"{controller}_{action}";
    }
}
