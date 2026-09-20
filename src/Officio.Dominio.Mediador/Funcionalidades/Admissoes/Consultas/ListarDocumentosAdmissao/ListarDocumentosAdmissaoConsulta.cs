using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarDocumentosAdmissao;

public record ListarDocumentosAdmissaoConsulta(Guid AdmissaoId)
    : IRequest<Resultado<List<AdmissaoDocumentoDto>>>;
