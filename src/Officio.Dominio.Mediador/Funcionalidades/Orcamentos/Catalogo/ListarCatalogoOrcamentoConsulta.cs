using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public record ListarCatalogoOrcamentoConsulta : IRequest<Resultado<List<CatalogoOrcamentoDto>>>;
