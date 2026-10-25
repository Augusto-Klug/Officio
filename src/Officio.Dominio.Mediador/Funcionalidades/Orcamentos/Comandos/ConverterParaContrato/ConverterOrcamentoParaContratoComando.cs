using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;

public record ConverterOrcamentoParaContratoComando(
    Guid OrcamentoId,
    DateTime DataInicio,
    int DuracaoAnos,
    DateOnly? CompetenciaInicialHonorarios,
    bool ConfirmarSubstituicaoContratoAtivo,
    string? ClausulasExtras,
    Guid? RepresentanteSocioId = null) : IRequest<Resultado<Guid>>;
