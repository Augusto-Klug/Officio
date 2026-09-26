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

    public Task<bool> ParceiroAtivoAsync(Guid parceiroId, CancellationToken ct = default)
        => contexto.Parceiros.AnyAsync(
            parceiro => parceiro.Id == parceiroId && parceiro.Ativo,
            ct);
}
