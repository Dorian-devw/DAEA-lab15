using System;
using System.Collections.Generic;

namespace _13Serpentarios.Entidades.Models;

public partial class EspeciesSerpiente
{
    public int EspecieId { get; set; }

    public string NombreCientifico { get; set; } = null!;

    public string NombreComun { get; set; } = null!;

    public string Familia { get; set; } = null!;

    public string RegionOrigen { get; set; } = null!;

    public string TipoVenenoDominante { get; set; } = null!;

    public int NivelPeligrosidad { get; set; }

    public bool Activa { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<LotesVeneno> LotesVenenos { get; set; } = new List<LotesVeneno>();
}

