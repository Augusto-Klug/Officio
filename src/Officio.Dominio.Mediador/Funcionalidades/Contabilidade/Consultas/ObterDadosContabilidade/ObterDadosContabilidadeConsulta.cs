using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade.Consultas.ObterDadosContabilidade;

public sealed record ObterDadosContabilidadeConsulta : IRequest<Resultado<DadosContabilidadeDto>>;
