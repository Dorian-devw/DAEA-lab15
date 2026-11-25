using _13Serpentarios.Datos.Models;
using _13Serpentarios.Entidades.Models;
using Microsoft.EntityFrameworkCore;

namespace _13Serpentarios.Datos.Repositorios;

public class EspecieSerpienteRepositorio : IEspecieSerpienteRepositorio
{
    private readonly SerpentarioContext _context;

    public EspecieSerpienteRepositorio(SerpentarioContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EspeciesSerpiente>> ObtenerTodasAsync(
        string? familia,
        int? peligrosidad,
        bool? activa,
        string? busqueda)
    {
        IQueryable<EspeciesSerpiente> consulta = _context.EspeciesSerpientes
            .AsNoTracking()
            .Include(e => e.LotesVenenos);

        if (!string.IsNullOrWhiteSpace(familia))
        {
            string familiaFiltro = familia.Trim();
            consulta = consulta.Where(e => e.Familia.Contains(familiaFiltro));
        }

        if (peligrosidad.HasValue)
        {
            consulta = consulta.Where(e => e.NivelPeligrosidad == peligrosidad.Value);
        }

        if (activa.HasValue)
        {
            consulta = consulta.Where(e => e.Activa == activa.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            string texto = busqueda.Trim();
            consulta = consulta.Where(e =>
                e.NombreCientifico.Contains(texto) ||
                e.NombreComun.Contains(texto) ||
                e.TipoVenenoDominante.Contains(texto));
        }

        return await consulta
            .OrderBy(e => e.NombreCientifico)
            .ToListAsync();
    }

    public async Task<EspeciesSerpiente?> ObtenerPorIdAsync(int id)
    {
        return await _context.EspeciesSerpientes
            .Include(e => e.LotesVenenos)
            .FirstOrDefaultAsync(e => e.EspecieId == id);
    }

    public async Task<bool> ExisteNombreCientificoAsync(string nombreCientifico, int? excluirId = null)
    {
        string nombreNormalizado = nombreCientifico.Trim().ToLower();
        return await _context.EspeciesSerpientes.AnyAsync(e =>
            e.NombreCientifico.ToLower() == nombreNormalizado &&
            (!excluirId.HasValue || e.EspecieId != excluirId.Value));
    }

    public async Task<bool> TieneLotesAsociadosAsync(int especieId)
    {
        return await _context.LotesVenenos.AnyAsync(l => l.EspecieId == especieId);
    }

    public async Task<EspeciesSerpiente> CrearAsync(EspeciesSerpiente especie)
    {
        _context.EspeciesSerpientes.Add(especie);
        await _context.SaveChangesAsync();
        return especie;
    }

    public async Task ActualizarAsync(EspeciesSerpiente especie)
    {
        _context.EspeciesSerpientes.Update(especie);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(EspeciesSerpiente especie)
    {
        _context.EspeciesSerpientes.Remove(especie);
        await _context.SaveChangesAsync();
    }
}

