using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ConsultarCep;

public sealed record ConsultarCepConsulta(string Cep)
    : IRequest<Resultado<EnderecoCepLocalidade?>>;
