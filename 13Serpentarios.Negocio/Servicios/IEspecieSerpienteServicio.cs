using _13Serpentarios.Entidades.DTOs;

namespace _13Serpentarios.Negocio.Servicios;

public interface IEspecieSerpienteServicio
{
    Task<IEnumerable<EspecieSerpienteDto>> ObtenerTodasAsync(
        string? familia,
        int? peligrosidad,
        bool? activa,
        string? busqueda);

    Task<EspecieSerpienteDto> ObtenerPorIdAsync(int id);

    Task<EspecieSerpienteDto> CrearAsync(EspecieSerpienteCrearDto dto);

    Task ActualizarAsync(int id, EspecieSerpienteActualizarDto dto);

    Task EliminarAsync(int id);
}

