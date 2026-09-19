using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.AlterarPerfil;
using Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.AlterarStatus;
using Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.CancelarConvite;
using Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.ConvidarUsuario;
using Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.ReenviarConvite;
using Officio.Dominio.Mediador.Funcionalidades.Usuarios.Consultas.ListarConvites;
using Officio.Dominio.Mediador.Funcionalidades.Usuarios.Consultas.ListarUsuarios;
using Officio.Dominio.Seguranca;
using Officio.ViewModels.Usuarios;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize(Roles = PerfisAcesso.Admin)]
public class UsuariosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok((await mediator.Send(new ListarUsuariosConsulta(), cancellationToken)).Valor);

    [HttpGet("convites")]
    public async Task<IActionResult> ListarConvites(CancellationToken cancellationToken)
        => Ok((await mediator.Send(new ListarConvitesConsulta(), cancellationToken)).Valor);

    [HttpPost("convites")]
    public async Task<IActionResult> Convidar(
        [FromBody] ConvidarUsuarioViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ConvidarUsuarioComando(viewModel.Nome, viewModel.Email, viewModel.Perfil),
            cancellationToken);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(ListarConvites), resultado.Valor)
            : BadRequest(new { error = resultado.Erro });
    }

    [HttpPost("convites/{conviteId:guid}/reenviar")]
    public async Task<IActionResult> Reenviar(Guid conviteId, CancellationToken cancellationToken)
        => Resultado(await mediator.Send(new ReenviarConviteComando(conviteId), cancellationToken));

    [HttpPost("convites/{conviteId:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid conviteId, CancellationToken cancellationToken)
        => Resultado(await mediator.Send(new CancelarConviteComando(conviteId), cancellationToken));

    [HttpPatch("{usuarioId:guid}/perfil")]
    public async Task<IActionResult> AlterarPerfil(
        Guid usuarioId,
        [FromBody] AlterarPerfilUsuarioViewModel viewModel,
        CancellationToken cancellationToken)
        => Resultado(await mediator.Send(
            new AlterarPerfilUsuarioComando(usuarioId, viewModel.Perfil), cancellationToken));

    [HttpPatch("{usuarioId:guid}/status")]
    public async Task<IActionResult> AlterarStatus(
        Guid usuarioId,
        [FromBody] AlterarStatusUsuarioViewModel viewModel,
        CancellationToken cancellationToken)
        => Resultado(await mediator.Send(
            new AlterarStatusUsuarioComando(usuarioId, viewModel.Ativo), cancellationToken));

    private IActionResult Resultado<T>(Officio.Dominio.Contratos.Response.Resultado<T> resultado)
        => resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(new { error = resultado.Erro });
}
