using _13Serpentarios.Entidades.Models;

namespace _13Serpentarios.Datos.Repositorios;

public interface IEspecieSerpienteRepositorio
{
    Task<IEnumerable<EspeciesSerpiente>> ObtenerTodasAsync(
        string? familia,
        int? peligrosidad,
        bool? activa,
        string? busqueda);

    Task<EspeciesSerpiente?> ObtenerPorIdAsync(int id);

    Task<bool> ExisteNombreCientificoAsync(string nombreCientifico, int? excluirId = null);

    Task<bool> TieneLotesAsociadosAsync(int especieId);

    Task<EspeciesSerpiente> CrearAsync(EspeciesSerpiente especie);

    Task ActualizarAsync(EspeciesSerpiente especie);

    Task EliminarAsync(EspeciesSerpiente especie);
}

