namespace _13Serpentarios.Entidades.DTOs;

public class LoteVenenoDto
{
    public int LoteId { get; set; }
    public string CodigoLote { get; set; } = string.Empty;
    public int EspecieId { get; set; }
    public string EspecieNombreComun { get; set; } = string.Empty;
    public string EspecieNombreCientifico { get; set; } = string.Empty;
    public DateOnly FechaExtraccion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal CantidadMiligramos { get; set; }
    public decimal PurezaPorcentaje { get; set; }
    public string Presentacion { get; set; } = string.Empty;
    public decimal TemperaturaAlmacenamientoC { get; set; }
    public string EstadoCalidad { get; set; } = string.Empty;
    public string UsoDestino { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

