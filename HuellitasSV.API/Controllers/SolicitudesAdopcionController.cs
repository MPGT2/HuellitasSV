using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para el envío de solicitudes de adopción por parte de los usuarios (HU-7).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SolicitudesAdopcionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SolicitudesAdopcionController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public SolicitudesAdopcionController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene una solicitud de adopción por su identificador, para que el usuario siga su estado.
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    /// <returns>La solicitud encontrada o NotFound si no existe.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SolicitudAdopcion>> GetSolicitud(int id)
    {
        var solicitud = await _context.SolicitudesAdopcion
            .Include(s => s.Mascota)
            .FirstOrDefaultAsync(s => s.IdSolicitud == id);

        if (solicitud is null)
        {
            return NotFound();
        }

        return Ok(solicitud);
    }

    /// <summary>
    /// Registra una nueva solicitud de adopción con estado "Pendiente" y notifica al refugio.
    /// </summary>
    /// <remarks>
    /// Reglas de negocio:
    /// - Si el usuario ya tiene una solicitud pendiente para la misma mascota, se responde 409 Conflict.
    /// - La solicitud se crea siempre en estado "Pendiente".
    /// - Al crearse, se genera una notificación para el refugio de la mascota.
    /// </remarks>
    /// <param name="solicitud">Datos de la solicitud: mascota, usuario y datos de contacto confirmados.</param>
    /// <returns>La solicitud creada con su identificador, o un error de validación.</returns>
    [HttpPost]
    public async Task<ActionResult<SolicitudAdopcion>> PostSolicitud([FromBody] SolicitudAdopcion solicitud)
    {
        var mascota = await _context.Mascotas.FindAsync(solicitud.IdMascota);

        if (mascota is null)
        {
            return NotFound("La mascota indicada no existe.");
        }

        var yaTienePendiente = await _context.SolicitudesAdopcion.AnyAsync(s =>
            s.IdMascota == solicitud.IdMascota &&
            s.IdUsuario == solicitud.IdUsuario &&
            s.Estado == SolicitudEstado.Pendiente);

        if (yaTienePendiente)
        {
            return Conflict("Ya existe una solicitud pendiente tuya para esta mascota. Espera la respuesta del refugio.");
        }

        solicitud.IdSolicitud = 0;
        solicitud.Estado = SolicitudEstado.Pendiente;
        solicitud.ComentarioDecision = null;
        solicitud.FechaSolicitud = DateTime.UtcNow;
        solicitud.Mascota = null;
        solicitud.Usuario = null;

        _context.SolicitudesAdopcion.Add(solicitud);

        // Notificación al refugio que publicó la mascota.
        _context.Notificaciones.Add(new Notificacion
        {
            Mensaje = $"Nueva solicitud de adopción para {mascota.Nombre} (mascota #{mascota.IdMascota}).",
            IdRefugio = mascota.IdRefugio,
            FechaCreacion = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSolicitud),
            new { id = solicitud.IdSolicitud },
            solicitud);
    }
}
