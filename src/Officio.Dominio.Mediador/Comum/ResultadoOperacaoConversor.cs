using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Comum;

internal static class ResultadoOperacaoConversor
{
    public static Resultado<T> ParaResultado<T>(this ResultadoOperacao<T> resultado)
        => resultado.Sucesso
            ? Resultado<T>.CriarSucesso(resultado.Valor!)
            : Resultado<T>.CriarFalha(resultado.Erro!);

    public static Resultado<TDestino> ParaResultado<TOrigem, TDestino>(
        this ResultadoOperacao<TOrigem> resultado,
        Func<TOrigem, TDestino> mapear)
        => resultado.Sucesso
            ? Resultado<TDestino>.CriarSucesso(mapear(resultado.Valor!))
            : Resultado<TDestino>.CriarFalha(resultado.Erro!);

    public static Resultado<Unit> ParaUnit(this ResultadoOperacao<bool> resultado)
        => resultado.Sucesso
            ? Resultado<Unit>.CriarSucesso(Unit.Value)
            : Resultado<Unit>.CriarFalha(resultado.Erro!);
}
