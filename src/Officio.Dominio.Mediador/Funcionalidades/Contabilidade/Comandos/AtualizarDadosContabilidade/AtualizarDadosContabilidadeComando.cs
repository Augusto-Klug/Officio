using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade.Comandos.AtualizarDadosContabilidade;

public sealed record AtualizarDadosContabilidadeComando(DadosContabilidadeDto Dados)
    : IRequest<Resultado<DadosContabilidadeDto>>;
