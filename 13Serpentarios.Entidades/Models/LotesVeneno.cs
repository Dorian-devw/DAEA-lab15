using System;
using System.Collections.Generic;

namespace _13Serpentarios.Entidades.Models;

public partial class LotesVeneno
{
    public int LoteId { get; set; }

    public string CodigoLote { get; set; } = null!;

    public int EspecieId { get; set; }

    public DateOnly FechaExtraccion { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public decimal CantidadMiligramos { get; set; }

    public decimal PurezaPorcentaje { get; set; }

    public string Presentacion { get; set; } = null!;

    public decimal TemperaturaAlmacenamientoC { get; set; }

    public string EstadoCalidad { get; set; } = null!;

    public string UsoDestino { get; set; } = null!;

    public string? Observaciones { get; set; }

    public virtual EspeciesSerpiente Especie { get; set; } = null!;
}

