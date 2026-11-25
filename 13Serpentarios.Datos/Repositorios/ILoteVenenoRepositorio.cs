using _13Serpentarios.Entidades.Models;

namespace _13Serpentarios.Datos.Repositorios;

public interface ILoteVenenoRepositorio
{
    Task<IEnumerable<LotesVeneno>> ObtenerTodosAsync(
        int? especieId,
        string? estadoCalidad,
        string? presentacion,
        decimal? purezaMinima);

    Task<IEnumerable<LotesVeneno>> ObtenerPorEspecieIdAsync(int especieId);

    Task<LotesVeneno?> ObtenerPorIdAsync(int id);

    Task<bool> ExisteCodigoLoteAsync(string codigoLote, int? excluirId = null);

    Task<LotesVeneno> CrearAsync(LotesVeneno lote);

    Task ActualizarAsync(LotesVeneno lote);

    Task EliminarAsync(LotesVeneno lote);
}

