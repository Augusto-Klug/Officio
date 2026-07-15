namespace Officio.Dominio.Mediador.Comum.Excecoes;

public class ExcecaoNaoEncontrado : Exception
{
    public ExcecaoNaoEncontrado(string name, object key)
        : base($"Entidade \"{name}\" ({key}) nÃ£o encontrada.") { }
}





