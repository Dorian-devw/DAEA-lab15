using _13Serpentarios.Datos.Repositorios;
using _13Serpentarios.Entidades.DTOs;
using _13Serpentarios.Entidades.Models;
using _13Serpentarios.Negocio.Excepciones;
using AutoMapper;

namespace _13Serpentarios.Negocio.Servicios;

public class EspecieSerpienteServicio : IEspecieSerpienteServicio
{
    private readonly IEspecieSerpienteRepositorio _repositorio;
    private readonly IMapper _mapper;

    public EspecieSerpienteServicio(IEspecieSerpienteRepositorio repositorio, IMapper mapper)
    {
        _repositorio = repositorio;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EspecieSerpienteDto>> ObtenerTodasAsync(
        string? familia,
        int? peligrosidad,
        bool? activa,
        string? busqueda)
    {
        var especies = await _repositorio.ObtenerTodasAsync(familia, peligrosidad, activa, busqueda);
        return _mapper.Map<IEnumerable<EspecieSerpienteDto>>(especies);
    }

    public async Task<EspecieSerpienteDto> ObtenerPorIdAsync(int id)
    {
        var especie = await _repositorio.ObtenerPorIdAsync(id)
            ?? throw new RecursoNoEncontradoException($"No se encontró la especie con ID {id}.");

        return _mapper.Map<EspecieSerpienteDto>(especie);
    }

    public async Task<EspecieSerpienteDto> CrearAsync(EspecieSerpienteCrearDto dto)
    {
        if (await _repositorio.ExisteNombreCientificoAsync(dto.NombreCientifico))
        {
            throw new ReglaNegocioException(
                $"Ya existe una especie registrada con el nombre científico '{dto.NombreCientifico.Trim()}'.");
        }

        var entidad = _mapper.Map<EspeciesSerpiente>(dto);
        entidad.NombreCientifico = entidad.NombreCientifico.Trim();
        entidad.NombreComun = entidad.NombreComun.Trim();
        entidad.Familia = entidad.Familia.Trim();
        entidad.RegionOrigen = entidad.RegionOrigen.Trim();
        entidad.TipoVenenoDominante = entidad.TipoVenenoDominante.Trim();
        entidad.FechaRegistro = DateTime.Now;

        var creada = await _repositorio.CrearAsync(entidad);
        return _mapper.Map<EspecieSerpienteDto>(creada);
    }

    public async Task ActualizarAsync(int id, EspecieSerpienteActualizarDto dto)
    {
        var existente = await _repositorio.ObtenerPorIdAsync(id)
            ?? throw new RecursoNoEncontradoException($"No se encontró la especie con ID {id}.");

        if (await _repositorio.ExisteNombreCientificoAsync(dto.NombreCientifico, id))
        {
            throw new ReglaNegocioException(
                $"Ya existe otra especie registrada con el nombre científico '{dto.NombreCientifico.Trim()}'.");
        }

        _mapper.Map(dto, existente);
        existente.NombreCientifico = existente.NombreCientifico.Trim();
        existente.NombreComun = existente.NombreComun.Trim();
        existente.Familia = existente.Familia.Trim();
        existente.RegionOrigen = existente.RegionOrigen.Trim();
        existente.TipoVenenoDominante = existente.TipoVenenoDominante.Trim();

        await _repositorio.ActualizarAsync(existente);
    }

    public async Task EliminarAsync(int id)
    {
        var existente = await _repositorio.ObtenerPorIdAsync(id)
            ?? throw new RecursoNoEncontradoException($"No se encontró la especie con ID {id}.");

        if (await _repositorio.TieneLotesAsociadosAsync(id))
        {
            throw new ReglaNegocioException(
                $"No se puede eliminar la especie '{existente.NombreCientifico}' porque tiene lotes de veneno registrados en el banco.");
        }

        await _repositorio.EliminarAsync(existente);
    }
}

