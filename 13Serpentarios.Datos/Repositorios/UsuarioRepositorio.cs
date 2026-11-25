using _13Serpentarios.Datos.Models;
using _13Serpentarios.Entidades.Models;
using Microsoft.EntityFrameworkCore;

namespace _13Serpentarios.Datos.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly SerpentarioContext _context;

    public UsuarioRepositorio(SerpentarioContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower());
    }

    public async Task AgregarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
