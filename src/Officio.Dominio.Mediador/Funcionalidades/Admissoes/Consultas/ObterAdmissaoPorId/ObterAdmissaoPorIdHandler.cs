using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterAdmissaoPorId;

public class ObterAdmissaoPorIdHandler(IAdmissaoServico servico)
    : IRequestHandler<ObterAdmissaoPorIdConsulta, Resultado<AdmissaoDetalheDto>>
{
    public Task<Resultado<AdmissaoDetalheDto>> Handle(ObterAdmissaoPorIdConsulta request, CancellationToken ct)
        => servico.ObterPorIdAsync(request.AdmissaoId, ct);
}
