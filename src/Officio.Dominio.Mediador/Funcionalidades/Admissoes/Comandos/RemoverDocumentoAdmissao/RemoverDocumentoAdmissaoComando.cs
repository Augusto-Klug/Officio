using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RemoverDocumentoAdmissao;

public record RemoverDocumentoAdmissaoComando(Guid AdmissaoId, Guid AdmissaoDocumentoId)
    : IRequest<Resultado<bool>>;
