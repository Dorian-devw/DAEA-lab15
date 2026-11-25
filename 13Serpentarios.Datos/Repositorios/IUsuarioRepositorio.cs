using _13Serpentarios.Entidades.Models;

namespace _13Serpentarios.Datos.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task AgregarAsync(Usuario usuario);
    Task GuardarCambiosAsync();
}
