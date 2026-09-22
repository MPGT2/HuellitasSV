using System.ComponentModel.DataAnnotations;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para que los refugios reciban y visualicen los reportes de animales callejeros (HU-15).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportesRescateController : ControllerBase
{
    /// <summary>
    /// Radio de cobertura en kilómetros: el panel muestra los reportes a esta distancia o menos del refugio.
    /// </summary>
    private const double RadioCoberturaKm = 5.0;

    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ReportesRescateController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public ReportesRescateController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene el panel de reportes de rescate de un refugio: lista con foto, ubicación y estado.
    /// </summary>
    /// <remarks>
    /// - Si el refugio tiene ubicación, solo se muestran los reportes a 5 km o menos de él.
    /// - Sin parámetro <c>estado</c> se muestran todos; con <c>estado=Pendiente</c> se obtiene la lista
    ///   de pendientes (los reportes atendidos dejan de aparecer en ella).
    /// - Si no hay coincidencias se retorna una lista vacía.
    /// </remarks>
    /// <param name="refugioId">Identificador del refugio (obligatorio).</param>
    /// <param name="estado">Filtra por estado: Pendiente o Atendido (opcional).</param>
    /// <returns>Lista de reportes ordenada de más reciente a más antigua.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReporteAnimal>>> GetReportes(
        [FromQuery] long refugioId,
        [FromQuery] string? estado)
    {
        if (refugioId <= 0)
        {
            return BadRequest("Debe indicar el identificador del refugio (refugioId).");
        }

        var refugio = await _context.Refugio.FindAsync(refugioId);

        if (refugio is null)
        {
            return NotFound("El refugio indicado no existe.");
        }

        IQueryable<ReporteAnimal> consulta = _context.ReportesAnimales.AsQueryable();

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

        // Si el refugio tiene ubicación, se muestran únicamente los reportes cercanos (5 km).
        if (refugio.Latitud is not null && refugio.Longitud is not null)
        {
            reportes = reportes
                .Where(r => DistanciaKm(
                    refugio.Latitud!.Value,
                    refugio.Longitud!.Value,
                    r.Latitud!.Value,
                    r.Longitud!.Value) <= RadioCoberturaKm)
                .ToList();
        }

        return Ok(reportes);
    }

    /// <summary>
    /// Obtiene un reporte de rescate por su identificador, con el refugio que lo atendió si corresponde.
    /// </summary>
    /// <param name="id">Identificador del reporte.</param>
    /// <returns>El reporte encontrado o NotFound si no existe.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReporteAnimal>> GetReporte(int id)
    {
        var reporte = await _context.ReportesAnimales
            .Include(r => r.Refugio)
            .FirstOrDefaultAsync(r => r.IdReporte == id);

        if (reporte is null)
        {
            return NotFound();
        }

        return Ok(reporte);
    }

    /// <summary>
    /// Marca un reporte como "atendido": actualiza su estado y lo retira de la lista de pendientes.
    /// </summary>
    /// <remarks>El reporte queda asociado al refugio que confirmó la atención.</remarks>
    /// <param name="id">Identificador del reporte.</param>
    /// <param name="request">Refugio que atiende el reporte.</param>
    /// <returns>El reporte atendido, o un error si no cumple las reglas.</returns>
    [HttpPut("{id:int}/atendido")]
    public async Task<ActionResult<ReporteAnimal>> MarcarComoAtendido(
        int id,
        [FromBody] ReporteAtencionRequest request)
    {
        var reporte = await _context.ReportesAnimales.FindAsync(id);

        if (reporte is null)
        {
            return NotFound("El reporte indicado no existe.");
        }

        if (reporte.Estado == ReporteEstado.Atendido)
        {
            return BadRequest("El reporte ya fue atendido anteriormente.");
        }

        var refugio = await _context.Refugio.FindAsync(request.IdRefugio);

        if (refugio is null)
        {
            return NotFound("El refugio indicado no existe.");
        }

        reporte.Estado = ReporteEstado.Atendido;
        reporte.IdRefugio = refugio.IdRefugio;

        await _context.SaveChangesAsync();

        return Ok(reporte);
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

/// <summary>
/// Datos enviados por el refugio al confirmar la atención de un reporte de rescate.
/// </summary>
/// <param name="IdRefugio">Identificador del refugio que atiende el reporte.</param>
public record ReporteAtencionRequest(
    [Range(1L, long.MaxValue, ErrorMessage = "El IdRefugio es obligatorio.")]
    long IdRefugio);
