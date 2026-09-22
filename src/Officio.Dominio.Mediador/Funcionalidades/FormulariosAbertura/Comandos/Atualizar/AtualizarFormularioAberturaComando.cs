using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.FormulariosAbertura.Comandos.Atualizar;

public record AtualizarFormularioAberturaComando(AtualizarFormularioAberturaRequisicao Requisicao)
    : IRequest<Resultado<FormularioAberturaDto>>;
