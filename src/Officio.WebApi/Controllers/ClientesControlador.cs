using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/clientes/[action]")]
[Authorize]
public class ClientesControlador : ControllerBase
{
    // TODO: implementar endpoints GET, GET/{id}, PUT/{id}, PATCH/{id}/sci-code.
}
