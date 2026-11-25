using System.ComponentModel.DataAnnotations;

namespace _13Serpentarios.Entidades.DTOs;

public class EspecieSerpienteCrearDto
{
    [Required(ErrorMessage = "El nombre científico es obligatorio.")]
    [StringLength(120, MinimumLength = 4, ErrorMessage = "El nombre científico debe tener entre 4 y 120 caracteres.")]
    public string NombreCientifico { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre común es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre común debe tener entre 2 y 100 caracteres.")]
    public string NombreComun { get; set; } = string.Empty;

    [Required(ErrorMessage = "La familia taxonómica es obligatoria.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "La familia debe tener entre 3 y 60 caracteres.")]
    public string Familia { get; set; } = string.Empty;

    [Required(ErrorMessage = "La región de origen es obligatoria.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "La región de origen debe tener entre 3 y 120 caracteres.")]
    public string RegionOrigen { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de veneno dominante es obligatorio.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "El tipo de veneno debe tener entre 3 y 60 caracteres.")]
    public string TipoVenenoDominante { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "El nivel de peligrosidad debe estar entre 1 y 5.")]
    public int NivelPeligrosidad { get; set; }

    public bool Activa { get; set; } = true;
}

