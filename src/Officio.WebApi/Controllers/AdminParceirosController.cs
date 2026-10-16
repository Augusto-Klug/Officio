using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Seguranca;
using Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.CriarParceiroAdministrador;
using Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.AlterarStatusParceiro;
using Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.ConvidarAdministradorParceiro;
using Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.ReenviarConviteAdministrador;
using Officio.Dominio.Mediador.Funcionalidades.Parceiros.Consultas.ListarParceiros;
using Officio.ViewModels.Parceiros;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admin/parceiros")]
[Authorize(Roles = PerfisAcesso.PlatformAdmin)]
public class AdminParceirosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarParceirosConsulta(), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(new { error = resultado.Erro });
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarParceiroAdministradorViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new CriarParceiroAdministradorComando(
            viewModel.Cnpj,
            viewModel.RazaoSocial,
            viewModel.NomeFantasia,
            viewModel.EmailParceiro,
            viewModel.NomeAdministrador,
            viewModel.EmailAdministrador), cancellationToken);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(Criar), resultado.Valor)
            : Conflict(new { error = resultado.Erro });
    }

    [HttpPatch("{parceiroId:guid}/status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AlterarStatus(
        Guid parceiroId,
        [FromBody] AlterarStatusParceiroViewModel viewModel,
        CancellationToken cancellationToken)
        => Resultado(await mediator.Send(
            new AlterarStatusParceiroComando(parceiroId, viewModel.Ativo), cancellationToken));

    [HttpPost("{parceiroId:guid}/administrador/convites")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConvidarAdministrador(
        Guid parceiroId,
        [FromBody] ConvidarAdministradorParceiroViewModel viewModel,
        CancellationToken cancellationToken)
        => Resultado(await mediator.Send(
            new ConvidarAdministradorParceiroComando(parceiroId, viewModel.Nome, viewModel.Email),
            cancellationToken));

    [HttpPost("{parceiroId:guid}/administrador/reenviar-convite")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ReenviarConviteAdministrador(
        Guid parceiroId,
        CancellationToken cancellationToken)
        => Resultado(await mediator.Send(
            new ReenviarConviteAdministradorComando(parceiroId), cancellationToken));

    private IActionResult Resultado<T>(Officio.Dominio.Contratos.Response.Resultado<T> resultado)
        => resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(new { error = resultado.Erro });
}
