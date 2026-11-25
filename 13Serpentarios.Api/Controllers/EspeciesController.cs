using _13Serpentarios.Entidades.DTOs;
using _13Serpentarios.Negocio.Excepciones;
using _13Serpentarios.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _13Serpentarios.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EspeciesController : ControllerBase
{
    private readonly IEspecieSerpienteServicio _especieServicio;
    private readonly ILoteVenenoServicio _loteServicio;

    public EspeciesController(
        IEspecieSerpienteServicio especieServicio,
        ILoteVenenoServicio loteServicio)
    {
        _especieServicio = especieServicio;
        _loteServicio = loteServicio;
    }

    /// <summary>
    /// Catálogo público de especies de serpientes.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<EspecieSerpienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EspecieSerpienteDto>>> ObtenerTodas(
        [FromQuery] string? familia,
        [FromQuery] int? peligrosidad,
        [FromQuery] bool? activa,
        [FromQuery] string? busqueda)
    {
        var resultado = await _especieServicio.ObtenerTodasAsync(familia, peligrosidad, activa, busqueda);
        return Ok(resultado);
    }

    /// <summary>
    /// Consulta pública de especie por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EspecieSerpienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EspecieSerpienteDto>> ObtenerPorId(int id)
    {
        try
        {
            var especie = await _especieServicio.ObtenerPorIdAsync(id);
            return Ok(especie);
        }
        catch (RecursoNoEncontradoException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Lotes asociados a una especie. Requiere estar autenticado (cualquier rol interno).
    /// </summary>
    [HttpGet("{id:int}/lotes")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<LoteVenenoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<LoteVenenoDto>>> ObtenerLotesPorEspecie(int id)
    {
        try
        {
            var lotes = await _loteServicio.ObtenerPorEspecieIdAsync(id);
            return Ok(lotes);
        }
        catch (RecursoNoEncontradoException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Registro de nueva especie. Exclusivo para el rol Herpetologo.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Herpetologo")]
    [ProducesResponseType(typeof(EspecieSerpienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EspecieSerpienteDto>> Crear([FromBody] EspecieSerpienteCrearDto dto)
    {
        try
        {
            var creada = await _especieServicio.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.EspecieId }, creada);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Actualización de datos de una especie. Exclusivo para el rol Herpetologo.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Herpetologo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] EspecieSerpienteActualizarDto dto)
    {
        try
        {
            await _especieServicio.ActualizarAsync(id, dto);
            return NoContent();
        }
        catch (RecursoNoEncontradoException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Eliminación de una especie. Exclusivo para el rol Herpetologo.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Herpetologo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _especieServicio.EliminarAsync(id);
            return NoContent();
        }
        catch (RecursoNoEncontradoException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
