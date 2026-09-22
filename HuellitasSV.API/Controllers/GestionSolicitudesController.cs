using System.ComponentModel.DataAnnotations;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para que los refugios revisen y decidan sobre las solicitudes de adopción recibidas (HU-8).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GestionSolicitudesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="GestionSolicitudesController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public GestionSolicitudesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene las solicitudes de adopción de las mascotas de un refugio, con estado, mascota y datos del solicitante.
    /// </summary>
    /// <remarks>
    /// Si no hay coincidencias se retorna una lista vacía (la interfaz muestra "sin solicitudes pendientes").
    /// </remarks>
    /// <param name="refugioId">Identificador del refugio (obligatorio).</param>
    /// <param name="estado">Filtra por estado: Pendiente, Aprobada o Rechazada (opcional).</param>
    /// <returns>Lista de solicitudes ordenadas de más reciente a más antigua.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitudAdopcion>>> GetSolicitudes(
        [FromQuery] int refugioId,
        [FromQuery] string? estado)
    {
        if (refugioId <= 0)
        {
            return BadRequest("Debe indicar el identificador del refugio (refugioId).");
        }

        var refugioExiste = await _context.Refugios.AnyAsync(r => r.IdRefugio == refugioId);
        if (!refugioExiste)
        {
            return NotFound("El refugio indicado no existe.");
        }

        IQueryable<SolicitudAdopcion> consulta = _context.SolicitudesAdopcion
            .Where(s => s.Mascota!.IdRefugio == refugioId)
            .Include(s => s.Mascota)
            .Include(s => s.Usuario);

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (!Enum.TryParse<SolicitudEstado>(estado, ignoreCase: true, out var estadoEnum))
            {
                return BadRequest("Estado no válido. Valores permitidos: Pendiente, Aprobada, Rechazada.");
            }

            consulta = consulta.Where(s => s.Estado == estadoEnum);
        }

        var solicitudes = await consulta
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();

        return Ok(solicitudes);
    }

    /// <summary>
    /// Obtiene una solicitud específica con los datos de la mascota y del solicitante.
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    /// <returns>La solicitud encontrada o NotFound si no existe.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SolicitudAdopcion>> GetSolicitud(int id)
    {
        var solicitud = await _context.SolicitudesAdopcion
            .Include(s => s.Mascota)
            .Include(s => s.Usuario)
            .FirstOrDefaultAsync(s => s.IdSolicitud == id);

        if (solicitud is null)
        {
            return NotFound();
        }

        return Ok(solicitud);
    }

    /// <summary>
    /// Aprueba una solicitud pendiente: cambia la mascota a "EnProcesoAdopcion" y notifica al usuario.
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    /// <param name="request">Refugio que decide y comentario opcional.</param>
    /// <returns>La solicitud aprobada, o un error si no cumple las reglas.</returns>
    [HttpPut("{id:int}/aprobar")]
    public async Task<ActionResult<SolicitudAdopcion>> AprobarSolicitud(
        int id,
        [FromBody] SolicitudDecisionRequest request)
    {
        return await EjecutarDecision(id, request, aprobar: true);
    }

    /// <summary>
    /// Rechaza una solicitud pendiente: notifica al usuario y deja la mascota disponible
    /// (si no existe otra solicitud aprobada para ella).
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    /// <param name="request">Refugio que decide y comentario opcional.</param>
    /// <returns>La solicitud rechazada, o un error si no cumple las reglas.</returns>
    [HttpPut("{id:int}/rechazar")]
    public async Task<ActionResult<SolicitudAdopcion>> RechazarSolicitud(
        int id,
        [FromBody] SolicitudDecisionRequest request)
    {
        return await EjecutarDecision(id, request, aprobar: false);
    }

    /// <summary>
    /// Valida y aplica la decisión del refugio sobre una solicitud pendiente.
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    /// <param name="request">Datos de la decisión.</param>
    /// <param name="aprobar">True para aprobar, False para rechazar.</param>
    /// <returns>La solicitud con el estado actualizado o el error correspondiente.</returns>
    private async Task<ActionResult<SolicitudAdopcion>> EjecutarDecision(
        int id,
        SolicitudDecisionRequest request,
        bool aprobar)
    {
        var solicitud = await _context.SolicitudesAdopcion.FindAsync(id);

        if (solicitud is null)
        {
            return NotFound("La solicitud indicada no existe.");
        }

        if (solicitud.Estado != SolicitudEstado.Pendiente)
        {
            return BadRequest($"Solo se pueden decidir solicitudes pendientes; el estado actual es \"{solicitud.Estado}\".");
        }

        var mascota = await _context.Mascotas.FindAsync(solicitud.IdMascota);

        if (mascota is null)
        {
            return NotFound("La mascota de la solicitud ya no existe.");
        }

        if (mascota.IdRefugio != request.IdRefugio)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "La mascota no pertenece al refugio indicado; este refugio no puede decidir sobre la solicitud.");
        }

        if (aprobar)
        {
            solicitud.Estado = SolicitudEstado.Aprobada;
            solicitud.ComentarioDecision = request.ComentarioDecision;
            mascota.Estado = MascotaEstado.EnProcesoAdopcion;

            _context.Notificaciones.Add(new Notificacion
            {
                Mensaje = $"Tu solicitud de adopción para {mascota.Nombre} fue APROBADA por el refugio. Pronto recibirás novedades.",
                IdUsuario = solicitud.IdUsuario,
                FechaCreacion = DateTime.UtcNow
            });
        }
        else
        {
            solicitud.Estado = SolicitudEstado.Rechazada;
            solicitud.ComentarioDecision = request.ComentarioDecision;

            // Si no queda ninguna otra aprobación en trámite, la mascota vuelve a estar disponible.
            var otraAprobada = await _context.SolicitudesAdopcion.AnyAsync(s =>
                s.IdMascota == mascota.IdMascota &&
                s.IdSolicitud != solicitud.IdSolicitud &&
                s.Estado == SolicitudEstado.Aprobada);

            if (!otraAprobada && mascota.Estado == MascotaEstado.EnProcesoAdopcion)
            {
                mascota.Estado = MascotaEstado.Disponible;
            }

            _context.Notificaciones.Add(new Notificacion
            {
                Mensaje = $"Tu solicitud de adopción para {mascota.Nombre} fue RECHAZADA por el refugio.",
                IdUsuario = solicitud.IdUsuario,
                FechaCreacion = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return Ok(solicitud);
    }
}

/// <summary>
/// Datos enviados por el refugio al aprobar o rechazar una solicitud de adopción.
/// </summary>
/// <param name="IdRefugio">Identificador del refugio que toma la decisión (debe ser dueño de la mascota).</param>
/// <param name="ComentarioDecision">Comentario opcional para el solicitante.</param>
public record SolicitudDecisionRequest(
    [property: Range(1, int.MaxValue, ErrorMessage = "El IdRefugio es obligatorio.")]
    int IdRefugio,

    [property: StringLength(500, ErrorMessage = "El comentario no puede exceder 500 caracteres.")]
    string? ComentarioDecision);
