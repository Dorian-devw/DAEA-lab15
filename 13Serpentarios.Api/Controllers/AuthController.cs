using System.Security.Claims;
using _13Serpentarios.Entidades.DTOs;
using _13Serpentarios.Negocio.Excepciones;
using _13Serpentarios.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _13Serpentarios.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _authServicio;

    public AuthController(IAuthServicio authServicio)
    {
        _authServicio = authServicio;
    }

    /// <summary>
    /// Registro de un nuevo usuario en el sistema. El rol lo asigna el servidor.
    /// </summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UsuarioPerfilDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registro([FromBody] UsuarioRegistroDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var usuarioCreado = await _authServicio.RegistrarAsync(dto);
            return StatusCode(StatusCodes.Status201Created, usuarioCreado);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Inicio de sesión de usuarios. Devuelve 401 tanto si el correo no existe como si la clave es incorrecta.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] UsuarioLoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tokenDto = await _authServicio.IniciarSesionAsync(dto);
        if (tokenDto == null)
        {
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });
        }

        return Ok(tokenDto);
    }

    /// <summary>
    /// Consulta del perfil del usuario autenticado a partir de los claims del token JWT (sin consultar la BD).
    /// </summary>
    [HttpGet("perfil")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioPerfilDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ObtenerPerfil()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email")
            ?? string.Empty;

        var rol = User.FindFirstValue(ClaimTypes.Role)
            ?? User.FindFirstValue("rol")
            ?? User.FindFirstValue("role")
            ?? string.Empty;

        return Ok(new UsuarioPerfilDto
        {
            Correo = correo,
            Rol = rol
        });
    }
}
