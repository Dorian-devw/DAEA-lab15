using _13Serpentarios.Entidades.DTOs;
using _13Serpentarios.Negocio.Excepciones;
using _13Serpentarios.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _13Serpentarios.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Base: todos los endpoints de lotes requieren token JWT
public class LotesVenenoController : ControllerBase
{
    private readonly ILoteVenenoServicio _loteServicio;

    public LotesVenenoController(ILoteVenenoServicio loteServicio)
    {
        _loteServicio = loteServicio;
    }

    /// <summary>
    /// Consulta de lotes de veneno. Requiere estar autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoteVenenoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<LoteVenenoDto>>> ObtenerTodos(
        [FromQuery] int? especieId,
        [FromQuery] string? estadoCalidad,
        [FromQuery] string? presentacion,
        [FromQuery] decimal? purezaMinima)
    {
        var resultado = await _loteServicio.ObtenerTodosAsync(especieId, estadoCalidad, presentacion, purezaMinima);
        return Ok(resultado);
    }

    /// <summary>
    /// Detalle de un lote por ID. Requiere estar autenticado.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LoteVenenoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoteVenenoDto>> ObtenerPorId(int id)
    {
        try
        {
            var lote = await _loteServicio.ObtenerPorIdAsync(id);
            return Ok(lote);
        }
        catch (RecursoNoEncontradoException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Registro de lote de veneno extraído. Permitido para Herpetologo y TecnicoLaboratorio.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Herpetologo,TecnicoLaboratorio")]
    [ProducesResponseType(typeof(LoteVenenoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoteVenenoDto>> Crear([FromBody] LoteVenenoCrearDto dto)
    {
        try
        {
            var creado = await _loteServicio.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.LoteId }, creado);
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
    /// Actualización de parámetros de calidad y almacenamiento del lote. Exclusivo para TecnicoLaboratorio.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "TecnicoLaboratorio")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] LoteVenenoActualizarDto dto)
    {
        try
        {
            await _loteServicio.ActualizarAsync(id, dto);
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
    /// Baja física o descarte de un lote de veneno. Exclusivo para Herpetologo.
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
            await _loteServicio.EliminarAsync(id);
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
