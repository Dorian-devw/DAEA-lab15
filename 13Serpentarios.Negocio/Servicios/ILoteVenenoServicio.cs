using _13Serpentarios.Entidades.DTOs;

namespace _13Serpentarios.Negocio.Servicios;

public interface ILoteVenenoServicio
{
    Task<IEnumerable<LoteVenenoDto>> ObtenerTodosAsync(
        int? especieId,
        string? estadoCalidad,
        string? presentacion,
        decimal? purezaMinima);

    Task<IEnumerable<LoteVenenoDto>> ObtenerPorEspecieIdAsync(int especieId);

    Task<LoteVenenoDto> ObtenerPorIdAsync(int id);

    Task<LoteVenenoDto> CrearAsync(LoteVenenoCrearDto dto);

    Task ActualizarAsync(int id, LoteVenenoActualizarDto dto);

    Task EliminarAsync(int id);
}

