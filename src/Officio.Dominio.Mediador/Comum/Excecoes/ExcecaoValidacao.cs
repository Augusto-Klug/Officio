using FluentValidation.Results;

namespace Officio.Dominio.Mediador.Comum.Excecoes;

public class ExcecaoValidacao : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ExcecaoValidacao() : base("Uma ou mais falhas de validaÃ§Ã£o ocorreram.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ExcecaoValidacao(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}





