namespace _13Serpentarios.Negocio.Excepciones;

/// <summary>
/// Excepción lanzada cuando se incumple una regla de negocio del serpentario o banco de venenos.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }
}

