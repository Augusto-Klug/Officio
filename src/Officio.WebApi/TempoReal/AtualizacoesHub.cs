using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Officio.Dominio.Seguranca;

namespace Officio.WebApi.TempoReal;

[Authorize(Roles = PerfisAcesso.Admin + "," + PerfisAcesso.Comercial + "," + PerfisAcesso.Operacional + "," + PerfisAcesso.Financeiro + "," + PerfisAcesso.Consulta)]
public sealed class AtualizacoesHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenant = Context.User?.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenant, out var parceiroId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GrupoParceiro(parceiroId),
            Context.ConnectionAborted);
        await base.OnConnectedAsync();
    }

    public static string GrupoParceiro(Guid parceiroId)
        => $"parceiro:{parceiroId:N}";
}

public sealed record AtualizacaoTempoRealMensagem(string Entidade, Guid? Id);
