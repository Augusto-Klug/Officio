using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaFacil.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController(IMediator mediator) : ControllerBase
{
    // TODO: implementar endpoints GET, GET/{id}, PUT/{id}, PATCH/{id}/sci-code
}
