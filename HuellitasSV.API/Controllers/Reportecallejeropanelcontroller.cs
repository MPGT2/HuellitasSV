using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// [SEGURIDAD] Solo accesible con token JWT de rol "Refugio"; el refugio se toma del token.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Refugio")]
public class ReporteCallejeroPanelController : ControllerBase
{
    private const double RadioCoberturaKm = 5.0;

    private readonly ApplicationDbContext _context;

    public ReporteCallejeroPanelController(ApplicationDbContext context)
    {
        _context = context;
    }

   
    /// <param name="refugioId">Identificador del refugio (obligatorio).</param>
    /// <param name="estado">Filtra por estado: Pendiente o Atendido (opcional).</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReporteAnimal>>> Listar(
        [FromQuery] long? refugioId,
        [FromQuery] string? estado)
    {
        // [SEGURIDAD] El refugio autenticado se toma del token JWT (se ignora el query del cliente).
        var refugioIdEfectivo = long.TryParse(User.FindFirstValue("idRefugio"), out var idRefugioToken)
            ? idRefugioToken
            : 0;

        if (refugioIdEfectivo <= 0)
        {
            return Unauthorized(new { error = "El token no incluye el refugio asociado." });
        }

        var refugio = await _context.Refugio.FindAsync(refugioIdEfectivo);
        if (refugio is null)
        {
            return NotFound("El refugio indicado no existe.");
        }

        IQueryable<ReporteAnimal> consulta = _context.ReportesAnimales;

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (!Enum.TryParse<ReporteEstado>(estado, ignoreCase: true, out var estadoEnum))
            {
                return BadRequest("Estado no válido. Valores permitidos: Pendiente, Atendido.");
            }
            consulta = consulta.Where(r => r.Estado == estadoEnum);
        }

        var reportes = await consulta
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();

        if (refugio.Latitud is not null && refugio.Longitud is not null)
        {
            reportes = reportes
                .Where(r => CalcularDistanciaKm(
                    refugio.Latitud.Value, refugio.Longitud.Value,
                    r.Latitud!.Value, r.Longitud!.Value) <= RadioCoberturaKm)
                .ToList();
        }

        return Ok(reportes);
    }

    
    [HttpPut("{id:int}/atender")]
    public async Task<ActionResult<ReporteAnimal>> MarcarComoAtendido(
        int id, [FromBody] AtencionReporteRequest request)
    {
        // [SEGURIDAD] El refugio que atiende se toma del token JWT (se ignora el IdRefugio del body).
        var idRefugioToken = long.TryParse(User.FindFirstValue("idRefugio"), out var idRefugioClaim)
            ? idRefugioClaim
            : 0;

        if (idRefugioToken <= 0)
        {
            return Unauthorized(new { error = "El token no incluye el refugio asociado." });
        }

        var reporte = await _context.ReportesAnimales.FindAsync(id);
        if (reporte is null)
        {
            return NotFound("El reporte indicado no existe.");
        }

        if (reporte.Estado == ReporteEstado.Atendido)
        {
            return BadRequest("El reporte ya fue atendido anteriormente.");
        }

        var refugio = await _context.Refugio.FindAsync(idRefugioToken);
        if (refugio is null)
        {
            return NotFound("El refugio indicado no existe.");
        }

        reporte.Estado = ReporteEstado.Atendido;
        reporte.IdRefugio = refugio.IdRefugio;

        await _context.SaveChangesAsync();
        return Ok(reporte);
    }

    private static double CalcularDistanciaKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double radioTierraKm = 6371;
        var deltaLat = GradosARadianes(lat2 - lat1);
        var deltaLon = GradosARadianes(lon2 - lon1);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(GradosARadianes(lat1)) * Math.Cos(GradosARadianes(lat2)) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        return 2 * radioTierraKm * Math.Asin(Math.Sqrt(a));
    }

    private static double GradosARadianes(double grados) => grados * Math.PI / 180.0;
}


public record AtencionReporteRequest(
    [Range(1L, long.MaxValue, ErrorMessage = "El IdRefugio es obligatorio.")]
    long IdRefugio);