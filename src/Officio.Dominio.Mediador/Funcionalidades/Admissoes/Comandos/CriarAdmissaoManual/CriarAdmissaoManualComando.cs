using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CriarAdmissaoManual;

public record CriarAdmissaoManualComando(Guid ClienteId, DadosAdmissaoRequisicao Dados)
    : IRequest<Resultado<Guid>>;
