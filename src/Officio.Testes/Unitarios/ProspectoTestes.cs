using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Eventos;
using Xunit;

namespace Officio.Testes.Unitarios;

public class ProspectoTestes
{
    [Fact]
    public void Criar_DeveIniciarProspectoEmNegociacao()
    {
        var tenantId = Guid.NewGuid();

        var prospecto = Prospecto.Create(
            tenantId,
            "Empresa Teste",
            "12.345.678/0001-90",
            RegimeTributario.SimplesNacional);

        Assert.Equal(tenantId, prospecto.TenantId);
        Assert.Equal("Empresa Teste", prospecto.RazaoSocial);
        Assert.Equal(StatusProspecto.EmNegociacao, prospecto.Status);
        Assert.Empty(prospecto.EventosDominio);
    }

    [Fact]
    public void MarkAsConverted_DeveAtualizarEstadoERegistrarEvento()
    {
        var prospecto = Prospecto.Create(
            Guid.NewGuid(),
            "Empresa Teste",
            null,
            null);
        var clienteId = Guid.NewGuid();

        prospecto.MarkAsConverted(clienteId);

        Assert.Equal(StatusProspecto.Convertido, prospecto.Status);
        Assert.Equal(clienteId, prospecto.ClientId);
        Assert.NotNull(prospecto.ConvertidoEm);
        Assert.Contains(prospecto.EventosDominio, evento => evento is EventoProspectoConvertido);
    }

    [Fact]
    public void MarkAsConverted_DeveBloquearConversaoDuplicada()
    {
        var prospecto = Prospecto.Create(
            Guid.NewGuid(),
            "Empresa Teste",
            null,
            null);

        prospecto.MarkAsConverted(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => prospecto.MarkAsConverted(Guid.NewGuid()));
    }
}
