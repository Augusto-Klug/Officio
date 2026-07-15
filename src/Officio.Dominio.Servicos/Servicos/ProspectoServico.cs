using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Servicos.Comum;
using Officio.Dominio.Servicos.Interfaces;
using Officio.Dominio.Servicos.Modelos;

namespace Officio.Dominio.Servicos.Servicos;

public class ProspectoServico(
    DbContext db,
    IEmpresaAtual empresaAtual) : IProspectoServico
{
    public async Task<ResultadoOperacao<ProspectoCriadoResultado>> CriarAsync(
        CriarProspectoRequisicao requisicao,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(requisicao.Cnpj))
        {
            var existe = await db.Set<Prospecto>()
                .AnyAsync(p => p.Cnpj == requisicao.Cnpj, ct);

            if (existe)
                return ResultadoOperacao<ProspectoCriadoResultado>.CriarFalha(
                    "Já existe um prospecto ou cliente com este CNPJ.");
        }

        var prospecto = Prospecto.Create(
            empresaAtual.TenantId,
            requisicao.RazaoSocial,
            requisicao.Cnpj,
            requisicao.RegimeTributario);

        db.Set<Prospecto>().Add(prospecto);

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

                db.Set<Socio>().Add(socio);
            }
        }

        await db.SaveChangesAsync(ct);

        return ResultadoOperacao<ProspectoCriadoResultado>.CriarSucesso(
            new ProspectoCriadoResultado(prospecto.Id, prospecto.RazaoSocial));
    }

    public async Task<ResultadoOperacao<Guid>> ConverterParaClienteAsync(
        Guid prospectoId,
        CancellationToken ct = default)
    {
        var prospecto = await db.Set<Prospecto>()
            .Include(p => p.Socios)
            .FirstOrDefaultAsync(p =>
                p.Id == prospectoId &&
                p.TenantId == empresaAtual.TenantId, ct);

        if (prospecto is null)
            return ResultadoOperacao<Guid>.CriarFalha("Prospecto não encontrado.");

        if (prospecto.Status == StatusProspecto.Convertido)
            return ResultadoOperacao<Guid>.CriarFalha("Prospecto já foi convertido.");

        var cliente = Cliente.CreateFromProspect(prospecto);
        db.Set<Cliente>().Add(cliente);

        foreach (var socio in prospecto.Socios)
            cliente.AddSocio(Socio.CopyFor(socio, clientId: cliente.Id));

        prospecto.MarkAsConverted(cliente.Id);

        await db.SaveChangesAsync(ct);

        return ResultadoOperacao<Guid>.CriarSucesso(cliente.Id);
    }

    public async Task<ResultadoOperacao<ResultadoPaginado<ProspectoResumo>>> ObterAsync(
        ObterProspectosFiltro filtro,
        CancellationToken ct = default)
    {
        var query = db.Set<Prospecto>().AsQueryable();

        if (!string.IsNullOrEmpty(filtro.Busca))
            query = query.Where(p =>
                p.RazaoSocial.Contains(filtro.Busca) ||
                (p.Cnpj != null && p.Cnpj.Contains(filtro.Busca)));

        if (filtro.ResponsavelId.HasValue)
            query = query.Where(p => p.ResponsavelId == filtro.ResponsavelId);

        var totalRegistros = await query.CountAsync(ct);
        var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)filtro.TamanhoPagina);

        var dados = await query
            .OrderByDescending(p => p.CriadoEm)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(p => new ProspectoResumo(
                p.Id,
                p.RazaoSocial,
                p.Cnpj,
                p.Status.ToString(),
                null,
                p.CriadoEm))
            .ToListAsync(ct);

        return ResultadoOperacao<ResultadoPaginado<ProspectoResumo>>.CriarSucesso(
            new ResultadoPaginado<ProspectoResumo>(
                dados,
                totalRegistros,
                filtro.Pagina,
                filtro.TamanhoPagina,
                totalPaginas));
    }
}
