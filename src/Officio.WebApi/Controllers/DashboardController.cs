using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ListarAlertas;
using Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ObterProjecaoReceita;
using Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ObterVisaoGeral;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/dashboard/[action]")]
[Authorize]
public class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> VisaoGeral(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterVisaoGeralDashboardConsulta(), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpGet]
    public async Task<IActionResult> Alertas(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarAlertasDashboardConsulta(), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpGet]
    public async Task<IActionResult> ProjecaoReceita(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterProjecaoReceitaDashboardConsulta(), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }
}
