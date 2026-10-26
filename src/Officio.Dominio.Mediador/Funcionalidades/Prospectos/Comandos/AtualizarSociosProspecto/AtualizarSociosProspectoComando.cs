using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarSociosProspecto;

public sealed record AtualizarSociosProspectoComando(Guid Id, CriarProspectoRequisicao Dados)
    : IRequest<Resultado<bool>>;
