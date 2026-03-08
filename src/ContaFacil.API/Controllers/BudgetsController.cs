using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaFacil.API.Controllers;

[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController(IMediator mediator) : ControllerBase
{
    // TODO: implementar endpoints GET/by-prospect/{id}, POST, PATCH/{id}/status, POST/{id}/to-contract
}
