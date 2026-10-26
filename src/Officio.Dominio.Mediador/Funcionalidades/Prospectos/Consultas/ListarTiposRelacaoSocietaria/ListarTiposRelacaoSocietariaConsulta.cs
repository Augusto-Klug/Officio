using MediatR;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ListarTiposRelacaoSocietaria;

public sealed record ListarTiposRelacaoSocietariaConsulta : IRequest<List<TipoRelacaoSocietariaDto>>;
