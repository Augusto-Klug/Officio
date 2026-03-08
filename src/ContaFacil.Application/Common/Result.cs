namespace ContaFacil.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public IEnumerable<string> ValidationErrors { get; init; } = [];

    private Result(bool success, T? value, string? error)
    {
        IsSuccess = success;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> ValidationFailure(IEnumerable<string> errors)
        => new(false, default, "Validation failed") { ValidationErrors = errors };
}
