using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarProspecto;

public record AtualizarProspectoComando(Guid Id, CriarProspectoRequisicao Requisicao) : IRequest<Resultado<bool>>;
