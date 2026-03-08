using ContaFacil.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ContaFacil.Infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
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
