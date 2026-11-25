using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using _13Serpentarios.Datos.Repositorios;
using _13Serpentarios.Entidades.DTOs;
using _13Serpentarios.Entidades.Models;
using _13Serpentarios.Negocio.Excepciones;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace _13Serpentarios.Negocio.Servicios;

public class AuthServicio : IAuthServicio
{
    private readonly IUsuarioRepositorio _usuarioRepo;
    private readonly IConfiguration _configuration;

    public AuthServicio(IUsuarioRepositorio usuarioRepo, IConfiguration configuration)
    {
        _usuarioRepo = usuarioRepo;
        _configuration = configuration;
    }

    public async Task<UsuarioPerfilDto> RegistrarAsync(UsuarioRegistroDto dto)
    {
        var existente = await _usuarioRepo.ObtenerPorCorreoAsync(dto.Correo);
        if (existente != null)
        {
            throw new ReglaNegocioException("El correo ya se encuentra registrado.");
        }

        string claveHash = BCrypt.Net.BCrypt.HashPassword(dto.Clave);

        // El rol lo decide exclusivamente el servidor (nunca el cliente)
        var usuario = new Usuario
        {
            Correo = dto.Correo.Trim().ToLower(),
            ClaveHash = claveHash,
            Rol = "TecnicoLaboratorio", // Rol inicial predeterminado del negocio
            FechaRegistro = DateTime.UtcNow
        };

        await _usuarioRepo.AgregarAsync(usuario);
        await _usuarioRepo.GuardarCambiosAsync();

        return new UsuarioPerfilDto
        {
            Correo = usuario.Correo,
            Rol = usuario.Rol
        };
    }

    public async Task<TokenDto?> IniciarSesionAsync(UsuarioLoginDto dto)
    {
        var usuario = await _usuarioRepo.ObtenerPorCorreoAsync(dto.Correo);
        if (usuario == null)
        {
            return null; // El controlador responderá 401 Unauthorized
        }

        bool claveValida = BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.ClaveHash);
        if (!claveValida)
        {
            return null; // El controlador responderá 401 Unauthorized
        }

        var jwtSection = _configuration.GetSection("Jwt");
        var claveSecreta = jwtSection["Key"]
            ?? throw new InvalidOperationException("La clave secreta JWT no está configurada.");
        var emisor = jwtSection["Issuer"];
        var audiencia = jwtSection["Audience"];
        var duracionMinutos = double.Parse(jwtSection["DuracionMinutos"] ?? "60");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(claveSecreta);
        var expiracion = DateTime.UtcNow.AddMinutes(duracionMinutos);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("rol", usuario.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiracion,
            Issuer = emisor,
            Audience = audiencia,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new TokenDto
        {
            Token = tokenHandler.WriteToken(token),
            Expiracion = expiracion
        };
    }
}
