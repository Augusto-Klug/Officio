using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Servicos.Servicos;

internal class ProspectoServico(
    IProspectoRepositorio prospectoRepositorio,
    IEmpresaAtual empresaAtual) : IProspectoServico
{
    public async Task<ResultadoOperacao<ProspectoCriadoResultado>> CriarAsync(
        CriarProspectoRequisicao requisicao,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(requisicao.Cnpj))
        {
            var existe = await prospectoRepositorio.ExisteComCnpjAsync(requisicao.Cnpj, ct);

            if (existe)
                return ResultadoOperacao<ProspectoCriadoResultado>.CriarFalha(
                    "JÃ¡ existe um prospecto ou cliente com este CNPJ.");
        }

        var prospecto = Prospecto.Create(
            empresaAtual.TenantId,
            requisicao.RazaoSocial,
            requisicao.Cnpj,
            requisicao.RegimeTributario);

        await prospectoRepositorio.AdicionarAsync(prospecto, ct);

        if (requisicao.Socios is { Count: > 0 })
        {
            foreach (var socioRequisicao in requisicao.Socios)
            {
                var socio = Socio.Create(
                    empresaAtual.TenantId,
                    socioRequisicao.Nome,
                    socioRequisicao.Cpf,
                    socioRequisicao.Participacao,
                    prospectId: prospecto.Id);

                await prospectoRepositorio.AdicionarSocioAsync(socio, ct);
            }
        }

        await prospectoRepositorio.SalvarAlteracoesAsync(ct);

        return ResultadoOperacao<ProspectoCriadoResultado>.CriarSucesso(
            new ProspectoCriadoResultado(prospecto.Id, prospecto.RazaoSocial));
    }

    public async Task<ResultadoOperacao<Guid>> ConverterParaClienteAsync(
        Guid prospectoId,
        CancellationToken ct = default)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(prospectoId, ct);

        if (prospecto is null)
            return ResultadoOperacao<Guid>.CriarFalha("Prospecto nÃ£o encontrado.");

        if (prospecto.Status == StatusProspecto.Convertido)
            return ResultadoOperacao<Guid>.CriarFalha("Prospecto jÃ¡ foi convertido.");

        var cliente = Cliente.CreateFromProspect(prospecto);
        await prospectoRepositorio.AdicionarAsync(cliente, ct);

        foreach (var socio in prospecto.Socios)
            cliente.AddSocio(Socio.CopyFor(socio, clientId: cliente.Id));

        prospecto.MarkAsConverted(cliente.Id);

        await prospectoRepositorio.SalvarAlteracoesAsync(ct);

        return ResultadoOperacao<Guid>.CriarSucesso(cliente.Id);
    }

    public async Task<ResultadoOperacao<ResultadoPaginado<ProspectoResumo>>> ObterAsync(
        ObterProspectosFiltro filtro,
        CancellationToken ct = default)
    {
        var query = prospectoRepositorio.MontarConsulta();

        if (!string.IsNullOrEmpty(filtro.Busca))
            query = query.Where(p =>
                p.RazaoSocial.Contains(filtro.Busca) ||
                (p.Cnpj != null && p.Cnpj.Contains(filtro.Busca)));

        if (!string.IsNullOrEmpty(filtro.Status) &&
            Enum.TryParse<StatusProspecto>(filtro.Status, ignoreCase: true, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (filtro.ResponsavelId.HasValue)
            query = query.Where(p => p.ResponsavelId == filtro.ResponsavelId);

        var totalRegistros = await prospectoRepositorio.ContarAsync(query, ct);
        var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)filtro.TamanhoPagina);

        var dados = prospectoRepositorio.ObterPagina(
            query,
            filtro.Pagina,
            filtro.TamanhoPagina);

        return ResultadoOperacao<ResultadoPaginado<ProspectoResumo>>.CriarSucesso(
            new ResultadoPaginado<ProspectoResumo>(
                dados,
                totalRegistros,
                filtro.Pagina,
                filtro.TamanhoPagina,
                totalPaginas));
    }
}
