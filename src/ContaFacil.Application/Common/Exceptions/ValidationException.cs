using FluentValidation.Results;

namespace ContaFacil.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("Uma ou mais falhas de validação ocorreram.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
