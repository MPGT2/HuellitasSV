using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para la consulta de notificaciones enviadas a refugios y usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="NotificacionesController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public NotificacionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene las notificaciones de un refugio y/o de un usuario, ordenadas de más reciente a más antigua.
    /// </summary>
    /// <param name="refugioId">Identificador del refugio destinatario (opcional).</param>
    /// <param name="usuarioId">Identificador del usuario destinatario (opcional).</param>
    /// <returns>Lista de notificaciones; vacía si no hay coincidencias.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notificacion>>> GetNotificaciones(
        [FromQuery] int? refugioId,
        [FromQuery] int? usuarioId)
    {
        IQueryable<Notificacion> consulta = _context.Notificaciones.AsQueryable();

        if (refugioId.HasValue)
        {
            consulta = consulta.Where(n => n.IdRefugio == refugioId.Value);
        }

        if (usuarioId.HasValue)
        {
            consulta = consulta.Where(n => n.IdUsuario == usuarioId.Value);
        }

        var notificaciones = await consulta
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync();

        return Ok(notificaciones);
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    /// <param name="id">Identificador de la notificación.</param>
    /// <returns>NoContent si se marcó; NotFound si no existe.</returns>
    [HttpPut("{id:int}/leida")]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);

        if (notificacion is null)
        {
            return NotFound();
        }

        notificacion.Leida = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
