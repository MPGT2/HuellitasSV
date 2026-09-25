using System.Security.Claims;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para la consulta de notificaciones enviadas a refugios y usuarios.
/// [SEGURIDAD] Solo accesible con token JWT (refugio o usuario); el destinatario se toma
/// del token, no de parámetros enviados por el cliente.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    /// Obtiene las notificaciones del destinatario autenticado (refugio o usuario según el token),
    /// ordenadas de más reciente a más antigua.
    /// </summary>
    /// <param name="leidas">Filtro opcional por estado de lectura (true: leídas, false: no leídas).</param>
    /// <returns>Lista de notificaciones; vacía si no hay coincidencias.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notificacion>>> GetNotificaciones([FromQuery] bool? leidas)
    {
        IQueryable<Notificacion> consulta = _context.Notificaciones.AsQueryable();

        // [SEGURIDAD] El destinatario se toma del token JWT: refugio o usuario según el rol.
        if (long.TryParse(User.FindFirstValue("idRefugio"), out var idRefugio))
        {
            consulta = consulta.Where(n => n.IdRefugio == idRefugio);
        }
        else if (long.TryParse(User.FindFirstValue("idUsuario"), out var idUsuario))
        {
            consulta = consulta.Where(n => n.IdUsuario == idUsuario);
        }
        else
        {
            return Unauthorized(new { error = "El token no incluye un destinatario válido." });
        }

        if (leidas.HasValue)
        {
            consulta = consulta.Where(n => n.Leida == leidas.Value);
        }

        var notificaciones = await consulta
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync();

        return Ok(notificaciones);
    }

    /// <summary>
    /// Marca una notificación como leída (solo si pertenece al destinatario autenticado).
    /// </summary>
    /// <param name="id">Identificador de la notificación.</param>
    /// <returns>NoContent si se marcó; NotFound si no existe; 403 si no es del destinatario.</returns>
    [HttpPut("{id:int}/leida")]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);

        if (notificacion is null)
        {
            return NotFound();
        }

        // [SEGURIDAD] Solo el destinatario de la notificación puede marcarla como leída.
        var esPropietaria =
            (long.TryParse(User.FindFirstValue("idRefugio"), out var idRefugio)
                && notificacion.IdRefugio == idRefugio)
            || (long.TryParse(User.FindFirstValue("idUsuario"), out var idUsuario)
                && notificacion.IdUsuario == idUsuario);

        if (!esPropietaria)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "La notificación no le pertenece." });
        }

        notificacion.Leida = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}