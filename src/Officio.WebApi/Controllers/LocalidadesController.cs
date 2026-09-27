using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ConsultarCep;
using Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ListarEstados;
using Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ListarMunicipios;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/localidades")]
[Authorize]
public sealed class LocalidadesController(IMediator mediator) : ControllerBase
{
    [HttpGet("estados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ListarEstados(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarEstadosConsulta(), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : StatusCode(503, resultado.Erro);
    }

    [HttpGet("estados/{uf}/municipios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarMunicipios(string uf, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarMunicipiosConsulta(uf), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpGet("cep/{cep}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ConsultarCep(string cep, CancellationToken cancellationToken)
    {
        if (new string(cep.Where(char.IsDigit).ToArray()).Length != 8)
            return BadRequest("Informe um CEP com 8 digitos.");

        var resultado = await mediator.Send(new ConsultarCepConsulta(cep), cancellationToken);
        if (!resultado.Sucesso)
            return StatusCode(503, resultado.Erro);

        return resultado.Valor is null
            ? NotFound("CEP nao encontrado. Confira os numeros informados.")
            : Ok(resultado.Valor);
    }
}
