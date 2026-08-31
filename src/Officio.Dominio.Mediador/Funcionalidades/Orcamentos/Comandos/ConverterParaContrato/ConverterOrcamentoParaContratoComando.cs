using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;

public record ConverterOrcamentoParaContratoComando(
    Guid OrcamentoId,
    DateTime DataInicio,
    DateTime? DataFim,
    bool RenovacaoAutomatica,
    string? ClausulasExtras) : IRequest<Resultado<Guid>>;
