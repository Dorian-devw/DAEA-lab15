namespace _13Serpentarios.Negocio.Excepciones;

/// <summary>
/// Excepción lanzada cuando el recurso solicitado o referenciado no existe en la base de datos.
/// </summary>
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string mensaje) : base(mensaje)
    {
    }
}

