using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/orcamentos/[action]")]
[Authorize]
public class OrcamentosControlador : ControllerBase
{
    // TODO: implementar endpoints GET/by-prospect/{id}, POST, PATCH/{id}/status, POST/{id}/to-contract.
}
