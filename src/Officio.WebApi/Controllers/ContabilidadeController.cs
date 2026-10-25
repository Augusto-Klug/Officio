using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Funcionalidades.Contabilidade;
using Officio.Dominio.Seguranca;
using Officio.ViewModels.Parceiros;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/contabilidade")]
[Authorize(Roles = PerfisAcesso.Admin)]
public class ContabilidadeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken ct)
    {
        var resultado = await mediator.Send(new ObterDadosContabilidadeConsulta(), ct);
        return resultado.Sucesso ? Ok(resultado.Valor) : NotFound(new { error = resultado.Erro });
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar(AtualizarDadosContabilidadeViewModel dados, CancellationToken ct)
    {
        var resultado = await mediator.Send(new AtualizarDadosContabilidadeComando(new DadosContabilidadeDto(
            dados.RazaoSocial, dados.NomeFantasia, dados.Cnpj, dados.Crc, dados.Email, dados.Telefone,
            MapearEndereco(dados.Endereco), dados.ResponsavelTecnico is { } r
                ? new(r.Nome, r.Cpf, r.Profissao, r.Crc, MapearEndereco(r.Endereco)) : null)), ct);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(new { error = resultado.Erro });
    }

    private static EnderecoContabilidadeDto? MapearEndereco(EnderecoContabilidadeViewModel? e) => e is null ? null
        : new(e.Cep, e.Logradouro, e.Numero, e.Complemento, e.Bairro, e.Cidade, e.Uf, e.MunicipioCodigoIbge);
}
