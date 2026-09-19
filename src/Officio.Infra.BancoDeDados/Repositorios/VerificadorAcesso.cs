using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

public class VerificadorAcesso(ContextoAplicacao contexto) : IVerificadorAcesso
{
    public Task<bool> UsuarioAtivoAsync(Guid usuarioId, CancellationToken ct = default)
        => contexto.Users.IgnoreQueryFilters().AnyAsync(
            usuario => usuario.Id == usuarioId && usuario.Ativo,
            ct);

    public async Task<bool> EmpresaAtivaAsync(Guid empresaId, CancellationToken ct = default)
        // Mantem compatibilidade com usuarios de desenvolvimento anteriores ao cadastro formal de tenants.
        => !await contexto.Empresas.AnyAsync(
            empresa => empresa.Id == empresaId && !empresa.Ativo,
            ct);
}
