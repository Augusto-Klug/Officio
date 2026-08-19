using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Officio.Dominio.Interfaces;

namespace Officio.Infra.BancoDeDados.Contextos;

public sealed class ContextoAplicacaoFabricaDesignTime : IDesignTimeDbContextFactory<ContextoAplicacao>
{
    public ContextoAplicacao CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=Officio;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseSqlServer(connectionString, configuracao =>
                configuracao.MigrationsAssembly(typeof(ContextoAplicacao).Assembly.FullName))
            .Options;

        return new ContextoAplicacao(options, new EmpresaAtualDesignTime(), new PublicadorEventosNulo());
    }

    private sealed class EmpresaAtualDesignTime : IEmpresaAtual
    {
        public Guid TenantId => Guid.Empty;

        public void SetTenant(Guid tenantId) { }
    }

    private sealed class PublicadorEventosNulo : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
