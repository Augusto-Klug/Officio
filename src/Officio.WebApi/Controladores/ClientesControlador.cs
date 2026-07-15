using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Officio.WebApi.Controladores;

[ApiController]
[Controller]
[Route("api/clientes")]
[Authorize]
public class ClientesControlador(IMediator mediator) : ControllerBase
{
    // TODO: implementar endpoints GET, GET/{id}, PUT/{id}, PATCH/{id}/sci-code
}






