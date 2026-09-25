using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para que los refugios revisen y decidan sobre las solicitudes de adopción recibidas (HU-8).
/// [SEGURIDAD] Solo accesible con token JWT de rol "Refugio"; el refugio se toma del token,
/// no de parámetros enviados por el cliente.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Refugio")]
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
    /// <param name="refugioId">Obsoleto: se ignora. El refugio se determina a partir del token JWT.</param>
    /// <param name="estado">Filtra por estado: Pendiente, Aprobada o Rechazada (opcional).</param>
    /// <returns>Lista de solicitudes ordenada de más reciente a más antigua.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitudAdopcion>>> GetSolicitudes(
        [FromQuery] long? refugioId,
        [FromQuery] string? estado)
    {
        // [SEGURIDAD] El refugio autenticado se toma del token JWT (no del query del cliente).
        var refugioIdEfectivo = long.TryParse(User.FindFirstValue("idRefugio"), out var idRefugioToken)
            ? idRefugioToken
            : 0;

        if (refugioIdEfectivo <= 0)
        {
            return Unauthorized(new { error = "El token no incluye el refugio asociado." });
        }

        var refugioExiste = await _context.Refugio.AnyAsync(r => r.IdRefugio == refugioIdEfectivo);
        if (!refugioExiste)
        {
            return NotFound("El refugio indicado no existe.");
        }

        IQueryable<SolicitudAdopcion> consulta = _context.SolicitudesAdopcion
            .Where(s => s.Mascota!.IdRefugio == refugioIdEfectivo)
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
    /// Aprueba una solicitud pendiente: reserva la mascota (estado "reservada") y notifica al usuario.
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
        // [SEGURIDAD] El refugio que decide se toma del token JWT (se ignora el IdRefugio del body).
        var idRefugioToken = long.TryParse(User.FindFirstValue("idRefugio"), out var idRefugioClaim)
            ? idRefugioClaim
            : 0;

        if (idRefugioToken <= 0)
        {
            return Unauthorized(new { error = "El token no incluye el refugio asociado." });
        }

        var solicitud = await _context.SolicitudesAdopcion.FindAsync(id);

        if (solicitud is null)
        {
            return NotFound("La solicitud indicada no existe.");
        }

        if (solicitud.Estado != SolicitudEstado.Pendiente)
        {
            return BadRequest($"Solo se pueden decidir solicitudes pendientes; el estado actual es \"{solicitud.Estado}\".");
        }

        var mascota = await _context.Mascota.FindAsync(solicitud.IdMascota);

        if (mascota is null)
        {
            return NotFound("La mascota de la solicitud ya no existe.");
        }

        if (mascota.IdRefugio != idRefugioToken)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "La mascota no pertenece a su refugio; no puede decidir sobre la solicitud.");
        }

        if (aprobar)
        {
            solicitud.Estado = SolicitudEstado.Aprobada;
            solicitud.ComentarioDecision = request.ComentarioDecision;

            // Estados de la tabla mascota: disponible, reservada, adoptada, en_tratamiento, fallecida.
            mascota.Estado = "reservada";

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

            if (!otraAprobada && mascota.Estado == "reservada")
            {
                mascota.Estado = "disponible";
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
    [Range(1L, long.MaxValue, ErrorMessage = "El IdRefugio es obligatorio.")]
    long IdRefugio,

    [StringLength(500, ErrorMessage = "El comentario no puede exceder 500 caracteres.")]
    string? ComentarioDecision);
