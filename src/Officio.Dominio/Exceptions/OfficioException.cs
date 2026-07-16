namespace Officio.Dominio.Exceptions;

public class OfficioException : Exception
{
    public OfficioException()
    {
    }

    public OfficioException(string message)
        : base(message)
    {
    }

    public OfficioException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
