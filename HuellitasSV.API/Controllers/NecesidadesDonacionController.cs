// [HU-XX] <Tu nombre>: Publicar necesidad urgente de insumos y cobertura automática.
// Endpoints: GET /api/NecesidadesDonacion, POST /api/NecesidadesDonacion, POST /api/NecesidadesDonacion/{id}/aportar

namespace HuellitasSV.API.Controllers;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasSV.API.Data;
using HuellitasSV.API.DTOs;
using HuellitasSV.API.Models;

/// <summary>
/// Controlador de necesidades urgentes de insumos publicadas por los refugios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NecesidadesDonacionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa el controlador con el contexto de base de datos.
    /// </summary>
    public NecesidadesDonacionController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista las necesidades de donación. Por defecto solo devuelve las "activas",
    /// que son las visibles para la comunidad. Use estado=todas para no filtrar.
    /// </summary>
    /// <response code="200">Lista de necesidades de donación.</response>
    [HttpGet]
    public async Task<IActionResult> GetNecesidades([FromQuery] string? estado = "activa")
    {
        var query = _context.NecesidadDonacion.AsQueryable();

        if (!string.IsNullOrEmpty(estado) && !estado.Equals("todas", StringComparison.OrdinalIgnoreCase))
            query = query.Where(n => n.Estado == estado.ToLower());

        var necesidades = await query
            .OrderByDescending(n => n.FechaPublicacion)
            .Select(n => new
            {
                n.IdNecesidad,
                n.IdRefugio,
                n.TipoInsumo,
                n.Descripcion,
                n.CantidadRequerida,
                n.CantidadCubierta,
                n.Estado,
                n.FechaPublicacion
            })
            .ToListAsync();

        return Ok(necesidades);
    }

    /// <summary>
    /// Publica una nueva necesidad urgente de insumos para un refugio.
    /// Criterio de aceptación: al completar tipo de insumo y cantidad requerida,
    /// la necesidad queda visible para los usuarios con estado "activa".
    /// </summary>
    /// <response code="201">Necesidad publicada.</response>
    /// <response code="400">Datos inválidos o el refugio no existe.</response>
    [HttpPost]
    public async Task<IActionResult> PublicarNecesidad([FromBody] PublicarNecesidadDto dto)
    {
        // El refugio debe existir antes de publicar una necesidad a su nombre.
        var refugioExiste = await _context.Refugio.AnyAsync(r => r.IdRefugio == dto.IdRefugio);
        if (!refugioExiste)
            return BadRequest(new { error = "El refugio especificado no existe." });

        var necesidad = new NecesidadDonacion
        {
            IdRefugio = dto.IdRefugio,
            TipoInsumo = dto.TipoInsumo.ToLower(),
            Descripcion = dto.Descripcion,
            CantidadRequerida = dto.CantidadRequerida,
            CantidadCubierta = 0,
            Estado = "activa",
            FechaPublicacion = DateTime.UtcNow
        };

        _context.NecesidadDonacion.Add(necesidad);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNecesidades), new { id = necesidad.IdNecesidad }, new
        {
            necesidad.IdNecesidad,
            necesidad.IdRefugio,
            necesidad.TipoInsumo,
            necesidad.Descripcion,
            necesidad.CantidadRequerida,
            necesidad.CantidadCubierta,
            necesidad.Estado,
            necesidad.FechaPublicacion
        });
    }

    /// <summary>
    /// Registra un aporte de la comunidad hacia una necesidad de donación.
    /// Criterio de aceptación: cuando la cantidad cubierta alcanza la cantidad
    /// requerida, el sistema marca la necesidad como "cubierta" automáticamente.
    /// </summary>
    /// <response code="200">Aporte registrado.</response>
    /// <response code="400">Cantidad inválida o la necesidad ya está cubierta.</response>
    /// <response code="404">Necesidad no encontrada.</response>
    [HttpPost("{id}/aportar")]
    public async Task<IActionResult> RegistrarAporte(long id, [FromBody] RegistrarAporteDto dto)
    {
        var necesidad = await _context.NecesidadDonacion.FindAsync(id);
        if (necesidad == null)
            return NotFound(new { error = "Necesidad de donación no encontrada." });

        if (necesidad.Estado == "cubierta")
            return BadRequest(new { error = "Esta necesidad ya fue cubierta y no acepta más aportes." });

        necesidad.CantidadCubierta += dto.Cantidad;

        // Cobertura automática: nunca se guarda por encima de lo requerido.
        if (necesidad.CantidadCubierta >= necesidad.CantidadRequerida)
        {
            necesidad.CantidadCubierta = necesidad.CantidadRequerida;
            necesidad.Estado = "cubierta";
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = necesidad.Estado == "cubierta"
                ? "¡Necesidad cubierta! Gracias por el aporte."
                : "Aporte registrado correctamente.",
            necesidad = new
            {
                necesidad.IdNecesidad,
                necesidad.CantidadRequerida,
                necesidad.CantidadCubierta,
                necesidad.Estado
            }
        });
    }
}