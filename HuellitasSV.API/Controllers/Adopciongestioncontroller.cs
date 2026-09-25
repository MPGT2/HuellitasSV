using System.ComponentModel.DataAnnotations;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdopcionGestionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdopcionGestionController(ApplicationDbContext context)
    {
        _context = context;
    }

   
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitudAdopcion>>> Listar(
        [FromQuery] long refugioId,
        [FromQuery] string? estado)
    {
        if (refugioId <= 0)
        {
            return BadRequest("Debe indicar el identificador del refugio (refugioId).");
        }

        var existeRefugio = await _context.Refugio.AnyAsync(r => r.IdRefugio == refugioId);
        if (!existeRefugio)
        {
            return NotFound("El refugio indicado no existe.");
        }

        var consulta = _context.SolicitudesAdopcion
            .Include(s => s.Mascota)
            .Include(s => s.Usuario)
            .Where(s => s.Mascota!.IdRefugio == refugioId)
            .AsQueryable();

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

   
    [HttpPut("{id:int}/aprobar")]
    public Task<ActionResult<SolicitudAdopcion>> Aprobar(int id, [FromBody] DecisionRefugioRequest request)
        => DecidirSolicitud(id, request, aprobar: true);


    [HttpPut("{id:int}/rechazar")]
    public Task<ActionResult<SolicitudAdopcion>> Rechazar(int id, [FromBody] DecisionRefugioRequest request)
        => DecidirSolicitud(id, request, aprobar: false);

    private async Task<ActionResult<SolicitudAdopcion>> DecidirSolicitud(
        int id, DecisionRefugioRequest request, bool aprobar)
    {
        var solicitud = await _context.SolicitudesAdopcion.FindAsync(id);
        if (solicitud is null)
        {
            return NotFound("La solicitud indicada no existe.");
        }

        if (solicitud.Estado != SolicitudEstado.Pendiente)
        {
            return BadRequest($"Solo se pueden decidir solicitudes pendientes (estado actual: \"{solicitud.Estado}\").");
        }

        var mascota = await _context.Mascota.FindAsync(solicitud.IdMascota);
        if (mascota is null)
        {
            return NotFound("La mascota de la solicitud ya no existe.");
        }

        if (mascota.IdRefugio != request.IdRefugio)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                "La mascota no pertenece al refugio indicado.");
        }

        solicitud.Estado = aprobar ? SolicitudEstado.Aprobada : SolicitudEstado.Rechazada;
        solicitud.ComentarioDecision = request.ComentarioDecision;

        if (aprobar)
        {
            mascota.Estado = "reservada";
        }
        else if (mascota.Estado == "reservada")
        {
            mascota.Estado = "disponible";
        }

        _context.Notificaciones.Add(new Notificacion
        {
            Mensaje = aprobar
                ? $"Tu solicitud para {mascota.Nombre} fue APROBADA."
                : $"Tu solicitud para {mascota.Nombre} fue RECHAZADA.",
            IdUsuario = solicitud.IdUsuario,
            FechaCreacion = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(solicitud);
    }
}

public record DecisionRefugioRequest(
    [Range(1L, long.MaxValue, ErrorMessage = "El IdRefugio es obligatorio.")]
    long IdRefugio,

    [StringLength(500)]
    string? ComentarioDecision);