using _13Serpentarios.Entidades.DTOs;

namespace _13Serpentarios.Negocio.Servicios;

public interface IAuthServicio
{
    Task<UsuarioPerfilDto> RegistrarAsync(UsuarioRegistroDto dto);
    Task<TokenDto?> IniciarSesionAsync(UsuarioLoginDto dto);
}
