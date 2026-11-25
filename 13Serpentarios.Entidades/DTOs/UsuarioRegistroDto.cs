using System.ComponentModel.DataAnnotations;

namespace _13Serpentarios.Entidades.DTOs;

public class UsuarioRegistroDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    [MinLength(6, ErrorMessage = "La clave debe tener al menos 6 caracteres.")]
    public string Clave { get; set; } = null!;
}
