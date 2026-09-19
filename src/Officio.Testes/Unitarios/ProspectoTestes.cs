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
        Assert.Equal("12345678000190", prospecto.Cnpj);
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

    [Fact]
    public void ConverterParaCliente_DeveCopiarDadosSociosERegistrarConversao()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(
            tenantId,
            "Empresa Teste",
            "12345678000190",
            RegimeTributario.SimplesNacional);
        prospecto.Socios.Add(Socio.Create(
            tenantId,
            "Socio Teste",
            "12345678901",
            100m,
            prospectId: prospecto.Id));

        var cliente = prospecto.ConverterParaCliente();

        Assert.Equal(prospecto.Id, cliente.ProspectOrigemId);
        Assert.Equal(prospecto.RazaoSocial, cliente.RazaoSocial);
        Assert.Equal(cliente.Id, prospecto.ClientId);
        Assert.Equal(StatusProspecto.Convertido, prospecto.Status);
        var socio = Assert.Single(cliente.Socios);
        Assert.Equal(cliente.Id, socio.ClientId);
        Assert.Equal("Socio Teste", socio.Nome);
        Assert.Contains(prospecto.EventosDominio, evento => evento is EventoProspectoConvertido);
    }
}
