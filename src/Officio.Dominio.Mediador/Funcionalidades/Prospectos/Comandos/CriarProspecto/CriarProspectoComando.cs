using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

public record CriarProspectoComando(CriarProspectoRequisicao Requisicao)
    : IRequest<Resultado<ProspectoCriadoDto>>;
