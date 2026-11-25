using System.ComponentModel.DataAnnotations;

namespace _13Serpentarios.Entidades.DTOs;

public class LoteVenenoCrearDto
{
    [Required(ErrorMessage = "El código de lote es obligatorio.")]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "El código de lote debe tener entre 4 y 30 caracteres.")]
    public string CodigoLote { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El identificador de especie debe ser mayor a 0.")]
    public int EspecieId { get; set; }

    [Required(ErrorMessage = "La fecha de extracción es obligatoria.")]
    public DateOnly FechaExtraccion { get; set; }

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    public DateOnly FechaVencimiento { get; set; }

    [Range(0.01, 100000.0, ErrorMessage = "La cantidad en miligramos debe estar entre 0.01 y 100000.")]
    public decimal CantidadMiligramos { get; set; }

    [Range(0.0, 100.0, ErrorMessage = "La pureza debe estar entre 0 y 100%.")]
    public decimal PurezaPorcentaje { get; set; }

    [Required(ErrorMessage = "La presentación es obligatoria.")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "La presentación debe tener entre 3 y 40 caracteres.")]
    public string Presentacion { get; set; } = string.Empty;

    [Range(-196.0, 25.0, ErrorMessage = "La temperatura de almacenamiento debe estar entre -196 °C y 25 °C.")]
    public decimal TemperaturaAlmacenamientoC { get; set; }

    [Required(ErrorMessage = "El estado de calidad es obligatorio.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El estado de calidad debe tener entre 3 y 30 caracteres.")]
    public string EstadoCalidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El uso destino es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El uso destino debe tener entre 3 y 120 caracteres.")]
    public string UsoDestino { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Las observaciones no pueden exceder los 300 caracteres.")]
    public string? Observaciones { get; set; }
}

