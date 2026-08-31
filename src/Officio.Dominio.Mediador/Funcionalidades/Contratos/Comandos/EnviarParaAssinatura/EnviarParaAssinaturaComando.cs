using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;

public record EnviarParaAssinaturaComando(EnviarAssinaturaRequisicao Requisicao)
    : IRequest<Resultado<SendForSignatureResultDto>>;
