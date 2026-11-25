using System.ComponentModel.DataAnnotations;

namespace _13Serpentarios.Entidades.DTOs;

public class UsuarioLoginDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    public string Clave { get; set; } = null!;
}
