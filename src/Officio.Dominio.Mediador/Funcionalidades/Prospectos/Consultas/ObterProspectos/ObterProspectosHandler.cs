using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;

internal class ObterProspectosHandler(
    IProspectoServico prospectoServico) : IRequestHandler<ObterProspectosConsulta, Resultado<PagedResult<ProspectoDto>>>
{
    public async Task<Resultado<PagedResult<ProspectoDto>>> Handle(
        ObterProspectosConsulta request,
        CancellationToken ct)
    {
        var resultado = await prospectoServico.ObterAsync(
            new ObterProspectosFiltro(
                request.Page,
                request.PageSize,
                request.Search,
                request.ResponsavelId),
            ct);

        if (!resultado.Sucesso)
            return Resultado<PagedResult<ProspectoDto>>.CriarFalha(resultado.Erro!);

        var pagina = resultado.Valor!;
        var dados = pagina.Dados
            .Select(p => new ProspectoDto(
                p.Id,
                p.RazaoSocial,
                p.Cnpj,
                p.Status,
                p.ResponsavelNome,
                p.CriadoEm))
            .ToList();

        return Resultado<PagedResult<ProspectoDto>>.CriarSucesso(
            new PagedResult<ProspectoDto>(
                dados,
                pagina.TotalRegistros,
                pagina.Pagina,
                pagina.TamanhoPagina,
                pagina.TotalPaginas));
    }
}
