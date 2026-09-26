using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Consultas.ListarParceiros;

public record ListarParceirosConsulta : IRequest<Resultado<List<ParceiroPlataformaDto>>>;
