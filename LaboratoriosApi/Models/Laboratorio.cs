namespace LaboratoriosApi.Models;

public class Laboratorio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ruc { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string CorreoContacto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool ActivoConvenio { get; set; }
}
