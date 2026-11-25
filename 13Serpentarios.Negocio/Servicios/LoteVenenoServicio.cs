using _13Serpentarios.Datos.Repositorios;
using _13Serpentarios.Entidades.DTOs;
using _13Serpentarios.Entidades.Models;
using _13Serpentarios.Negocio.Excepciones;
using AutoMapper;

namespace _13Serpentarios.Negocio.Servicios;

public class LoteVenenoServicio : ILoteVenenoServicio
{
    private const decimal PurezaMinimaAprobacion = 85.00m;
    private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cuarentena",
        "Aprobado",
        "Agotado",
        "Descartado"
    };

    private readonly ILoteVenenoRepositorio _loteRepositorio;
    private readonly IEspecieSerpienteRepositorio _especieRepositorio;
    private readonly IMapper _mapper;

    public LoteVenenoServicio(
        ILoteVenenoRepositorio loteRepositorio,
        IEspecieSerpienteRepositorio especieRepositorio,
        IMapper mapper)
    {
        _loteRepositorio = loteRepositorio;
        _especieRepositorio = especieRepositorio;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LoteVenenoDto>> ObtenerTodosAsync(
        int? especieId,
        string? estadoCalidad,
        string? presentacion,
        decimal? purezaMinima)
    {
        var lotes = await _loteRepositorio.ObtenerTodosAsync(especieId, estadoCalidad, presentacion, purezaMinima);
        return _mapper.Map<IEnumerable<LoteVenenoDto>>(lotes);
    }

    public async Task<IEnumerable<LoteVenenoDto>> ObtenerPorEspecieIdAsync(int especieId)
    {
        _ = await _especieRepositorio.ObtenerPorIdAsync(especieId)
            ?? throw new RecursoNoEncontradoException($"No se encontró la especie con ID {especieId}.");

        var lotes = await _loteRepositorio.ObtenerPorEspecieIdAsync(especieId);
        return _mapper.Map<IEnumerable<LoteVenenoDto>>(lotes);
    }

    public async Task<LoteVenenoDto> ObtenerPorIdAsync(int id)
    {
        var lote = await _loteRepositorio.ObtenerPorIdAsync(id)
            ?? throw new RecursoNoEncontradoException($"No se encontró el lote de veneno con ID {id}.");

        return _mapper.Map<LoteVenenoDto>(lote);
    }

    public async Task<LoteVenenoDto> CrearAsync(LoteVenenoCrearDto dto)
    {
        var especie = await _especieRepositorio.ObtenerPorIdAsync(dto.EspecieId)
            ?? throw new RecursoNoEncontradoException($"No se encontró la especie con ID {dto.EspecieId}.");

        if (!especie.Activa)
        {
            throw new ReglaNegocioException(
                $"No se pueden registrar nuevos lotes para la especie '{especie.NombreCientifico}' porque se encuentra inactiva o en cuarentena.");
        }

        if (await _loteRepositorio.ExisteCodigoLoteAsync(dto.CodigoLote))
        {
            throw new ReglaNegocioException(
                $"Ya existe un lote registrado con el código '{dto.CodigoLote.Trim()}'.");
        }

        ValidarReglasBioFarmaceuticas(
            dto.FechaExtraccion,
            dto.FechaVencimiento,
            dto.PurezaPorcentaje,
            dto.EstadoCalidad);

        var entidad = _mapper.Map<LotesVeneno>(dto);
        NormalizarCamposTexto(entidad);

        var creado = await _loteRepositorio.CrearAsync(entidad);
        return _mapper.Map<LoteVenenoDto>(creado);
    }

    public async Task ActualizarAsync(int id, LoteVenenoActualizarDto dto)
    {
        var existente = await _loteRepositorio.ObtenerPorIdAsync(id)
            ?? throw new RecursoNoEncontradoException($"No se encontró el lote de veneno con ID {id}.");

        var especie = await _especieRepositorio.ObtenerPorIdAsync(dto.EspecieId)
            ?? throw new RecursoNoEncontradoException($"No se encontró la especie con ID {dto.EspecieId}.");

        if (!especie.Activa && dto.EspecieId != existente.EspecieId)
        {
            throw new ReglaNegocioException(
                $"No se puede reasignar el lote a la especie inactiva '{especie.NombreCientifico}'.");
        }

        if (await _loteRepositorio.ExisteCodigoLoteAsync(dto.CodigoLote, id))
        {
            throw new ReglaNegocioException(
                $"Ya existe otro lote registrado con el código '{dto.CodigoLote.Trim()}'.");
        }

        ValidarReglasBioFarmaceuticas(
            dto.FechaExtraccion,
            dto.FechaVencimiento,
            dto.PurezaPorcentaje,
            dto.EstadoCalidad);

        _mapper.Map(dto, existente);
        existente.Especie = especie;
        NormalizarCamposTexto(existente);

        await _loteRepositorio.ActualizarAsync(existente);
    }

    public async Task EliminarAsync(int id)
    {
        var existente = await _loteRepositorio.ObtenerPorIdAsync(id)
            ?? throw new RecursoNoEncontradoException($"No se encontró el lote de veneno con ID {id}.");

        if (existente.EstadoCalidad.Equals("Aprobado", StringComparison.OrdinalIgnoreCase) &&
            existente.CantidadMiligramos > 0)
        {
            throw new ReglaNegocioException(
                $"No se puede eliminar el lote aprobado '{existente.CodigoLote}' porque aún conserva {existente.CantidadMiligramos} mg en stock. Márquelo como 'Agotado' o 'Descartado' primero.");
        }

        await _loteRepositorio.EliminarAsync(existente);
    }

    private static void ValidarReglasBioFarmaceuticas(
        DateOnly fechaExtraccion,
        DateOnly fechaVencimiento,
        decimal purezaPorcentaje,
        string estadoCalidad)
    {
        if (fechaVencimiento <= fechaExtraccion)
        {
            throw new ReglaNegocioException(
                "La fecha de vencimiento del lote debe ser posterior a la fecha de extracción.");
        }

        string estadoNormalizado = estadoCalidad.Trim();
        if (!EstadosValidos.Contains(estadoNormalizado))
        {
            throw new ReglaNegocioException(
                "El estado de calidad debe ser uno de los siguientes valores: Cuarentena, Aprobado, Agotado o Descartado.");
        }

        if (estadoNormalizado.Equals("Aprobado", StringComparison.OrdinalIgnoreCase))
        {
            if (purezaPorcentaje < PurezaMinimaAprobacion)
            {
                throw new ReglaNegocioException(
                    $"Un lote con pureza del {purezaPorcentaje}% no puede ser aprobado para uso farmacéutico (mínimo requerido: {PurezaMinimaAprobacion}%).");
            }

            if (fechaVencimiento <= DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ReglaNegocioException(
                    "No se puede aprobar un lote cuya fecha de vencimiento ya expiró.");
            }
        }
    }

    private static void NormalizarCamposTexto(LotesVeneno lote)
    {
        lote.CodigoLote = lote.CodigoLote.Trim().ToUpperInvariant();
        lote.Presentacion = lote.Presentacion.Trim();
        lote.EstadoCalidad = lote.EstadoCalidad.Trim();
        lote.UsoDestino = lote.UsoDestino.Trim();
        lote.Observaciones = string.IsNullOrWhiteSpace(lote.Observaciones) ? null : lote.Observaciones.Trim();
    }
}

