using Officio.Dominio.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Officio.Infraestrutura.Identidade;

public class ServicoUsuarioAtual(IHttpContextAccessor httpContextAccessor) : IUsuarioAtual
{
    public Guid UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }

    public string? Email => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
}





