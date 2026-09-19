using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AtualizarAdmissao;

public record AtualizarAdmissaoComando(Guid AdmissaoId, DadosAdmissaoRequisicao Dados)
    : IRequest<Resultado<Guid>>;
