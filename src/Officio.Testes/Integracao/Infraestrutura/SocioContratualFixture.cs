using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
namespace Officio.Testes.Integracao.Infraestrutura;
internal static class SocioContratualFixture
{
    public static Socio Adicionar(ContextoAplicacao contexto, Prospecto prospecto, string codigo = "SOCIO")
    {
        var socio = Socio.Create(prospecto.TenantId, "Ana Representante", "11789299900", null, prospectId: prospecto.Id);
        Completar(contexto, socio, codigo);
        contexto.Socios.Add(socio);
        return socio;
    }
    public static void Completar(ContextoAplicacao contexto, Socio socio, string codigo = "SOCIO")
    {
        var estado = contexto.Estados.Local.FirstOrDefault() ?? Estado.Criar("42", "SC", "Santa Catarina");
        var municipio = contexto.Municipios.Local.FirstOrDefault() ?? Municipio.Criar(estado.Id, "4202404", "Blumenau");
        var tipo = contexto.TiposRelacao.Local.FirstOrDefault(t => t.Codigo == codigo)
            ?? new TipoRelacao(codigo, codigo == "SOCIO" ? "Sócio" : "Sócio administrador", "SOCIETARIO");
        contexto.Estados.Add(estado); contexto.Municipios.Add(municipio); contexto.TiposRelacao.Add(tipo);
        var endereco = Endereco.Criar(municipio.Id, "Rua do Representante", "89010000", "10", "Centro");
        contexto.Set<Endereco>().Add(endereco);
        socio.Pessoa.AtualizarDadosSocietarios(socio.Nome, socio.Cpf, "Arquiteta", endereco);
        socio.AtualizarVinculo(socio.Participacao, tipo);
    }
}
