using _13Serpentarios.Datos.Models;
using _13Serpentarios.Entidades.Models;
using Microsoft.EntityFrameworkCore;

namespace _13Serpentarios.Datos.Repositorios;

public class LoteVenenoRepositorio : ILoteVenenoRepositorio
{
    private readonly SerpentarioContext _context;

    public LoteVenenoRepositorio(SerpentarioContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LotesVeneno>> ObtenerTodosAsync(
        int? especieId,
        string? estadoCalidad,
        string? presentacion,
        decimal? purezaMinima)
    {
        IQueryable<LotesVeneno> consulta = _context.LotesVenenos
            .AsNoTracking()
            .Include(l => l.Especie);

        if (especieId.HasValue)
        {
            consulta = consulta.Where(l => l.EspecieId == especieId.Value);
        }

        if (!string.IsNullOrWhiteSpace(estadoCalidad))
        {
            string estadoFiltro = estadoCalidad.Trim();
            consulta = consulta.Where(l => l.EstadoCalidad.Contains(estadoFiltro));
        }

        if (!string.IsNullOrWhiteSpace(presentacion))
        {
            string presentacionFiltro = presentacion.Trim();
            consulta = consulta.Where(l => l.Presentacion.Contains(presentacionFiltro));
        }

        if (purezaMinima.HasValue)
        {
            consulta = consulta.Where(l => l.PurezaPorcentaje >= purezaMinima.Value);
        }

        return await consulta
            .OrderByDescending(l => l.FechaExtraccion)
            .ThenBy(l => l.CodigoLote)
            .ToListAsync();
    }

    public async Task<IEnumerable<LotesVeneno>> ObtenerPorEspecieIdAsync(int especieId)
    {
        return await _context.LotesVenenos
            .AsNoTracking()
            .Include(l => l.Especie)
            .Where(l => l.EspecieId == especieId)
            .OrderByDescending(l => l.FechaExtraccion)
            .ToListAsync();
    }

    public async Task<LotesVeneno?> ObtenerPorIdAsync(int id)
    {
        return await _context.LotesVenenos
            .Include(l => l.Especie)
            .FirstOrDefaultAsync(l => l.LoteId == id);
    }

    public async Task<bool> ExisteCodigoLoteAsync(string codigoLote, int? excluirId = null)
    {
        string codigoNormalizado = codigoLote.Trim().ToLower();
        return await _context.LotesVenenos.AnyAsync(l =>
            l.CodigoLote.ToLower() == codigoNormalizado &&
            (!excluirId.HasValue || l.LoteId != excluirId.Value));
    }

    public async Task<LotesVeneno> CrearAsync(LotesVeneno lote)
    {
        _context.LotesVenenos.Add(lote);
        await _context.SaveChangesAsync();
        await _context.Entry(lote).Reference(l => l.Especie).LoadAsync();
        return lote;
    }

    public async Task ActualizarAsync(LotesVeneno lote)
    {
        _context.LotesVenenos.Update(lote);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(LotesVeneno lote)
    {
        _context.LotesVenenos.Remove(lote);
        await _context.SaveChangesAsync();
    }
}

