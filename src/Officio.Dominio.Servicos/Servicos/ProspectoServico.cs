using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Comum;

namespace Officio.Dominio.Servicos.Servicos;

internal class ProspectoServico(
    IProspectoRepositorio prospectoRepositorio,
    IEmpresaAtual empresaAtual) : IProspectoServico
{
    public async Task<Resultado<ProspectoCriadoDto>> CriarAsync(
        CriarProspectoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cnpj = DocumentoFiscal.SomenteDigitos(requisicao.Cnpj);

        if (cnpj is not null)
        {
            var existe = await prospectoRepositorio.ExisteComCnpjAsync(cnpj, ct);

            if (existe)
                return Resultado<ProspectoCriadoDto>.CriarFalha(
                    "JÃ¡ existe um prospecto ou cliente com este CNPJ.");
        }

        var prospecto = Prospecto.Create(
            empresaAtual.TenantId,
            requisicao.RazaoSocial,
            cnpj,
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

        return Resultado<ProspectoCriadoDto>.CriarSucesso(
            new ProspectoCriadoDto(prospecto.Id, prospecto.RazaoSocial));
    }

    public async Task<Resultado<Guid>> ConverterParaClienteAsync(
        Guid prospectoId,
        CancellationToken ct = default)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(prospectoId, ct);

        if (prospecto is null)
            return Resultado<Guid>.CriarFalha("Prospecto nÃ£o encontrado.");

        if (prospecto.Status == StatusProspecto.Convertido)
            return Resultado<Guid>.CriarFalha("Prospecto jÃ¡ foi convertido.");

        var cliente = prospecto.ConverterParaCliente();
        await prospectoRepositorio.AdicionarAsync(cliente, ct);

        await prospectoRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(cliente.Id);
    }

    public async Task<Resultado<PagedResult<ProspectoDto>>> ObterAsync(
        ObterProspectosFiltro filtro,
        CancellationToken ct = default)
    {
        var query = prospectoRepositorio.MontarConsulta();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim();
            var documento = DocumentoFiscal.SomenteDigitos(busca);

            query = query.Where(p =>
                p.RazaoSocial.Contains(busca) ||
                (p.Cnpj != null &&
                    (p.Cnpj.Contains(busca) ||
                     (documento != null && p.Cnpj
                         .Replace(".", "")
                         .Replace("/", "")
                         .Replace("-", "")
                         .Contains(documento)))));
        }

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

        return Resultado<PagedResult<ProspectoDto>>.CriarSucesso(
            new PagedResult<ProspectoDto>(
                dados
                    .Select(p => new ProspectoDto(
                        p.Id,
                        p.RazaoSocial,
                        p.Cnpj,
                        p.Status,
                        p.ResponsavelNome,
                        p.CriadoEm))
                    .ToList(),
                totalRegistros,
                filtro.Pagina,
                filtro.TamanhoPagina,
                totalPaginas));
    }
}
