namespace Officio.Dominio.Contratos.Response;

public class Resultado<T>
{
    public bool Sucesso { get; }
    public T? Valor { get; }
    public string? Erro { get; }
    public IEnumerable<string> ErrosValidacao { get; init; } = [];

    private Resultado(bool success, T? value, string? error)
    {
        Sucesso = success;
        Valor = value;
        Erro = error;
    }

    public static Resultado<T> CriarSucesso(T value) => new(true, value, null);

    public static Resultado<T> CriarFalha(string error) => new(false, default, error);

    public static Resultado<T> CriarFalhaValidacao(IEnumerable<string> errors)
        => new(false, default, "Validation failed") { ErrosValidacao = errors };
}
