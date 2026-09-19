using MediatR;
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class MapeamentoNotificacaoTestes
{
    [Fact]
    public void Modelo_ConfiguraIndiceUnicoParaNotificacaoReferenciada()
    {
        using var contexto = CriarContexto();
        var entidade = contexto.Model.FindEntityType(typeof(Notificacao))!;
        var indice = Assert.Single(
            entidade.GetIndexes(),
            item => item.Properties.Select(propriedade => propriedade.Name).SequenceEqual(
                [nameof(Notificacao.TenantId), nameof(Notificacao.UserId), nameof(Notificacao.Tipo), nameof(Notificacao.ReferenciaId)]));

        Assert.True(indice.IsUnique);
        Assert.Equal("[ReferenciaId] IS NOT NULL", indice.GetFilter());
        Assert.Equal(80, entidade.FindProperty(nameof(Notificacao.Tipo))!.GetMaxLength());
        Assert.Equal(200, entidade.FindProperty(nameof(Notificacao.Titulo))!.GetMaxLength());
        Assert.Equal(1000, entidade.FindProperty(nameof(Notificacao.Mensagem))!.GetMaxLength());
    }

    private static ContextoAplicacao CriarContexto()
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(Guid.NewGuid());

        return new ContextoAplicacao(options, empresaAtual, new PublicadorFake());
    }

    private sealed class PublicadorFake : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
