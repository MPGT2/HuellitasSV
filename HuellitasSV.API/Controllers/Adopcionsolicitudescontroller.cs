using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para que un usuario envíe una solicitud de adopción sobre una
/// mascota publicada en el catálogo (HU-7).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdopcionSolicitudesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdopcionSolicitudesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Consulta el estado de una solicitud de adopción por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SolicitudAdopcion>> ObtenerPorId(int id)
    {
        var solicitud = await _context.SolicitudesAdopcion
            .Include(s => s.Mascota)
            .FirstOrDefaultAsync(s => s.IdSolicitud == id);

        return solicitud is null ? NotFound() : Ok(solicitud);
    }

    /// <summary>
    /// Envía una nueva solicitud de adopción para una mascota disponible.
    /// </summary>
    /// <remarks>
    /// Reglas de negocio:
    /// - La mascota debe existir y estar en estado "disponible"; si ya fue reservada o adoptada,
    ///   se responde 409 Conflict para no generar solicitudes sobre mascotas no disponibles.
    /// - Si el mismo usuario ya tiene una solicitud "Pendiente" para la misma mascota, se rechaza
    ///   con 409 Conflict (evita duplicados).
    /// - La solicitud se crea en estado "Pendiente" y se notifica al refugio dueño de la mascota.
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<SolicitudAdopcion>> Crear([FromBody] SolicitudAdopcion solicitud)
    {
        var mascota = await _context.Mascota.FindAsync(solicitud.IdMascota);
        if (mascota is null)
        {
            return NotFound("La mascota indicada no existe.");
        }

        if (mascota.Estado != "disponible")
        {
            return Conflict($"La mascota no está disponible para adopción (estado actual: \"{mascota.Estado}\").");
        }

        var yaTieneSolicitudPendiente = await _context.SolicitudesAdopcion.AnyAsync(s =>
            s.IdMascota == solicitud.IdMascota &&
            s.IdUsuario == solicitud.IdUsuario &&
            s.Estado == SolicitudEstado.Pendiente);

        if (yaTieneSolicitudPendiente)
        {
            return Conflict("Ya tienes una solicitud pendiente para esta mascota.");
        }

        solicitud.IdSolicitud = 0;
        solicitud.Estado = SolicitudEstado.Pendiente;
        solicitud.ComentarioDecision = null;
        solicitud.FechaSolicitud = DateTime.UtcNow;
        solicitud.Mascota = null;
        solicitud.Usuario = null;

        _context.SolicitudesAdopcion.Add(solicitud);

        _context.Notificaciones.Add(new Notificacion
        {
            Mensaje = $"Nueva solicitud de adopción para {mascota.Nombre}.",
            IdRefugio = mascota.IdRefugio,
            FechaCreacion = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = solicitud.IdSolicitud }, solicitud);
    }
}