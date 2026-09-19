using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Seguranca;
using Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.CriarEmpresaAdministrador;
using Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.AlterarStatusEmpresa;
using Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.ReenviarConviteAdministrador;
using Officio.Dominio.Mediador.Funcionalidades.Empresas.Consultas.ListarEmpresas;
using Officio.ViewModels.Empresas;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admin/empresas")]
[Authorize(Roles = PerfisAcesso.PlatformAdmin)]
public class AdminEmpresasController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarEmpresasConsulta(), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(new { error = resultado.Erro });
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] CriarEmpresaAdministradorViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new CriarEmpresaAdministradorComando(
            viewModel.Cnpj,
            viewModel.RazaoSocial,
            viewModel.NomeFantasia,
            viewModel.EmailEmpresa,
            viewModel.NomeAdministrador,
            viewModel.EmailAdministrador), cancellationToken);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(Criar), resultado.Valor)
            : Conflict(new { error = resultado.Erro });
    }

    [HttpPatch("{empresaId:guid}/status")]
    public async Task<IActionResult> AlterarStatus(
        Guid empresaId,
        [FromBody] AlterarStatusEmpresaViewModel viewModel,
        CancellationToken cancellationToken)
        => Resultado(await mediator.Send(
            new AlterarStatusEmpresaComando(empresaId, viewModel.Ativa), cancellationToken));

    [HttpPost("{empresaId:guid}/administrador/reenviar-convite")]
    public async Task<IActionResult> ReenviarConviteAdministrador(
        Guid empresaId,
        CancellationToken cancellationToken)
        => Resultado(await mediator.Send(
            new ReenviarConviteAdministradorComando(empresaId), cancellationToken));

    private IActionResult Resultado<T>(Officio.Dominio.Contratos.Response.Resultado<T> resultado)
        => resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(new { error = resultado.Erro });
}
