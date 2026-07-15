using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Officio.WebApi.Controladores;

[ApiController]
[Controller]
[Route("api/orcamentos")]
[Authorize]
public class OrcamentosControlador(IMediator mediator) : ControllerBase
{
    // TODO: implementar endpoints GET/by-prospect/{id}, POST, PATCH/{id}/status, POST/{id}/to-contract
}






