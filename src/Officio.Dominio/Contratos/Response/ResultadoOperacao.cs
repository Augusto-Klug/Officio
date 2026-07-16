namespace Officio.Dominio.Contratos.Response;

public class ResultadoOperacao<T>
{
    public bool Sucesso { get; }
    public T? Valor { get; }
    public string? Erro { get; }

    private ResultadoOperacao(bool sucesso, T? valor, string? erro)
    {
        Sucesso = sucesso;
        Valor = valor;
        Erro = erro;
    }

    public static ResultadoOperacao<T> CriarSucesso(T valor) => new(true, valor, null);
    public static ResultadoOperacao<T> CriarFalha(string erro) => new(false, default, erro);
}
