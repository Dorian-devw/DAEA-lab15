namespace _13Serpentarios.Entidades.DTOs;

public class EspecieSerpienteDto
{
    public int EspecieId { get; set; }
    public string NombreCientifico { get; set; } = string.Empty;
    public string NombreComun { get; set; } = string.Empty;
    public string Familia { get; set; } = string.Empty;
    public string RegionOrigen { get; set; } = string.Empty;
    public string TipoVenenoDominante { get; set; } = string.Empty;
    public int NivelPeligrosidad { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaRegistro { get; set; }
    public int CantidadLotes { get; set; }
}

