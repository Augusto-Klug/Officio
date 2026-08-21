using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

public record CriarProspectoComando(CriarProspectoRequisicao Requisicao)
    : IRequest<Resultado<ProspectoCriadoDto>>;

public record ProspectoCriadoDto(Guid Id, string RazaoSocial);
