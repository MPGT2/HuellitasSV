using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para la creación de reportes de animales callejeros por parte de los usuarios (HU-14).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportesAnimalesController : ControllerBase
{
    /// <summary>
    /// Radio de notificación en kilómetros: los refugios a esta distancia o menos reciben la alerta.
    /// </summary>
    private const double RadioNotificacionKm = 5.0;

    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ReportesAnimalesController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public ReportesAnimalesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene un reporte de animal callejero por su identificador.
    /// </summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <returns>El reporte encontrado o NotFound si no existe.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReporteAnimal>> GetReporte(int id)
    {
        var reporte = await _context.ReportesAnimales.FindAsync(id);

        if (reporte is null)
        {
            return NotFound();
        }

        return Ok(reporte);
    }

    /// <summary>
    /// Registra un reporte de perro o gato callejero y notifica a los refugios cercanos a la ubicación.
    /// </summary>
    /// <remarks>
    /// Reglas de negocio:
    /// - La ubicación (latitud y longitud) es obligatoria: sin ella el sistema bloquea el reporte con 400.
    /// - La foto y la descripción también son obligatorias.
    /// - Al registrarse, el sistema calcula la distancia a cada refugio y notifica a los que estén
    ///   a 5 km o menos de la ubicación reportada.
    /// </remarks>
    /// <param name="reporte">Datos del reporte: usuario, descripción, foto y ubicación.</param>
    /// <returns>El reporte creado con estado "Pendiente", o un error de validación.</returns>
    [HttpPost]
    public async Task<ActionResult<ReporteAnimal>> PostReporte([FromBody] ReporteAnimal reporte)
    {
        // La validación de [Required] sobre Latitud/Longitud bloquea aquí con 400 si no hay ubicación.

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

        // Notificación a los refugios cercanos a la ubicación del reporte.
        var refugiosConUbicacion = await _context.Refugios
            .Where(r => r.Latitud != null && r.Longitud != null)
            .ToListAsync();

        foreach (var refugio in refugiosConUbicacion)
        {
            var distancia = DistanciaKm(
                reporte.Latitud!.Value,
                reporte.Longitud!.Value,
                refugio.Latitud!.Value,
                refugio.Longitud!.Value);

            if (distancia <= RadioNotificacionKm)
            {
                _context.Notificaciones.Add(new Notificacion
                {
                    Mensaje = $"Animal callejero reportado a {distancia:F1} km de tu refugio \"{refugio.Nombre}\". Revisa el panel de rescates.",
                    IdRefugio = refugio.IdRefugio,
                    FechaCreacion = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetReporte),
            new { id = reporte.IdReporte },
            reporte);
    }

    /// <summary>
    /// Calcula la distancia en kilómetros entre dos puntos geográficos usando la fórmula de Haversine.
    /// </summary>
    /// <param name="lat1">Latitud del primer punto.</param>
    /// <param name="lon1">Longitud del primer punto.</param>
    /// <param name="lat2">Latitud del segundo punto.</param>
    /// <param name="lon2">Longitud del segundo punto.</param>
    /// <returns>Distancia en kilómetros.</returns>
    private static double DistanciaKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double radioTierraKm = 6371;

        var deltaLat = GradosARadianes(lat2 - lat1);
        var deltaLon = GradosARadianes(lon2 - lon1);

        var a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(GradosARadianes(lat1)) * Math.Cos(GradosARadianes(lat2)) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        return 2 * radioTierraKm * Math.Asin(Math.Sqrt(a));
    }

    /// <summary>
    /// Convierte grados a radianes.
    /// </summary>
    private static double GradosARadianes(double grados) => grados * Math.PI / 180.0;
}
