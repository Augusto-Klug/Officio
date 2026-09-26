using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ConsultarCep;

internal sealed class ConsultarCepHandler(ILocalidadeServico servico)
    : IRequestHandler<ConsultarCepConsulta, Resultado<EnderecoCepLocalidade?>>
{
    public Task<Resultado<EnderecoCepLocalidade?>> Handle(
        ConsultarCepConsulta request,
        CancellationToken cancellationToken)
        => servico.ConsultarCepAsync(request.Cep, cancellationToken);
}
