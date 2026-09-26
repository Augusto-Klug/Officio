using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ListarMunicipios;

public sealed record ListarMunicipiosConsulta(string Uf)
    : IRequest<Resultado<IReadOnlyCollection<MunicipioLocalidade>>>;
