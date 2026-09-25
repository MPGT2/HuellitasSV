using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ReporteCallejeroController : ControllerBase
{
    private const double RadioAlertaKm = 5.0;

    private readonly ApplicationDbContext _context;

    public ReporteCallejeroController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReporteAnimal>> ObtenerPorId(int id)
    {
        var reporte = await _context.ReportesAnimales.FindAsync(id);
        return reporte is null ? NotFound() : Ok(reporte);
    }

    
    [HttpPost]
    public async Task<ActionResult<ReporteAnimal>> Crear([FromBody] ReporteAnimal reporte)
    {
        var usuario = await _context.Usuarios.FindAsync(reporte.IdUsuario);
        if (usuario is null)
        {
            return NotFound("El usuario indicado no existe.");
        }

        reporte.IdReporte = 0;
        reporte.IdRefugio = null;
        reporte.Estado = ReporteEstado.Pendiente;
        reporte.FechaRegistro = DateTime.UtcNow;
        reporte.Usuario = null;
        reporte.Refugio = null;

        _context.ReportesAnimales.Add(reporte);

        var refugiosCercanos = await _context.Refugio
            .Where(r => r.Latitud != null && r.Longitud != null)
            .ToListAsync();

        foreach (var refugio in refugiosCercanos)
        {
            var distancia = CalcularDistanciaKm(
                reporte.Latitud!.Value, reporte.Longitud!.Value,
                refugio.Latitud!.Value, refugio.Longitud!.Value);

            if (distancia <= RadioAlertaKm)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    Mensaje = $"Animal callejero reportado a {distancia:F1} km de \"{refugio.NombreOrganizacion}\".",
                    IdRefugio = refugio.IdRefugio,
                    FechaCreacion = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = reporte.IdReporte }, reporte);
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