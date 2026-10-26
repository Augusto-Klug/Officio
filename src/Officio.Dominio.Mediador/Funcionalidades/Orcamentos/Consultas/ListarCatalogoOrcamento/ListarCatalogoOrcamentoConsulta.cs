using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ListarCatalogoOrcamento;

public sealed record ListarCatalogoOrcamentoConsulta(bool IncluirInativos = false)
    : IRequest<Resultado<List<CatalogoOrcamentoDto>>>;
