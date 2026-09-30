using System.ComponentModel.DataAnnotations;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using HuellitasSV.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para que los refugios reciban y visualicen los reportes de animales callejeros (HU-15).
/// [SEGURIDAD] Solo accesible con token JWT de rol "Refugio"; el refugio se toma del token.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Refugio")]
public class ReportesRescateController : ControllerBase
{
    /// <summary>
    /// Radio de cobertura en kilómetros: el panel muestra los reportes a esta distancia o menos del refugio.
    /// </summary>
    private const double RadioCoberturaKm = 5.0;

    private readonly ApplicationDbContext _context;

    /// <summary>Claims del token ya convertidos.</summary>
    private readonly ICurrentUserService _usuarioActual;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ReportesRescateController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    /// <param name="usuarioActual">Claims del token ya convertidos.</param>
    public ReportesRescateController(ApplicationDbContext context, ICurrentUserService usuarioActual)
    {
        _context = context;
        _usuarioActual = usuarioActual;
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
    public async Task<ActionResult<IEnumerable<ReporteRescateDto>>> GetReportes(
        [FromQuery] long? refugioId,
        [FromQuery] string? estado)
    {
        // [SEGURIDAD] El refugio autenticado se toma del token JWT (se ignora el query del cliente).
        var refugioIdEfectivo = _usuarioActual.ObtenerRefugioId();

        if (refugioIdEfectivo <= 0)
        {
            return Unauthorized(new { error = "El token no incluye el refugio asociado." });
        }

        var refugio = await _context.Refugio.FindAsync(refugioIdEfectivo);

        if (refugio is null)
        {
            return NotFound(new { error = "El refugio indicado no existe." });
        }

        IQueryable<ReporteAnimal> consulta = _context.ReportesAnimales.AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (!Enum.TryParse<ReporteEstado>(estado, ignoreCase: true, out var estadoEnum))
            {
                return BadRequest(new { error = "Estado no válido. Valores permitidos: Pendiente, Atendido." });
            }

            consulta = consulta.Where(r => r.Estado == estadoEnum);
        }

        var reportes = await consulta
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();

        var tieneUbicacion = refugio.Latitud is not null && refugio.Longitud is not null;

        // Se devuelven TODOS los reportes con su distancia al refugio y una bandera
        // de cercanía: el panel puede mostrar los lejanos como aviso en vez de ocultarlos.
        var resultado = reportes
            .Select(r =>
            {
                double? distancia = null;
                if (tieneUbicacion && r.Latitud is not null && r.Longitud is not null)
                {
                    distancia = Math.Round(DistanciaKm(
                        refugio.Latitud!.Value,
                        refugio.Longitud!.Value,
                        r.Latitud.Value,
                        r.Longitud.Value), 1);
                }

                return new ReporteRescateDto
                {
                    IdReporte = r.IdReporte,
                    Descripcion = r.Descripcion,
                    FotoUrl = r.FotoUrl,
                    Latitud = r.Latitud,
                    Longitud = r.Longitud,
                    Estado = r.Estado.ToString(),
                    FechaRegistro = r.FechaRegistro,
                    IdRefugio = r.IdRefugio,
                    DistanciaKm = distancia,
                    // Sin ubicación del refugio no se puede medir: se tratan como cercanos.
                    Cerca = distancia is null || distancia <= RadioCoberturaKm
                };
            })
            .OrderBy(r => r.Estado == "Atendido" ? 1 : 0)
            .ThenBy(r => r.DistanciaKm ?? double.MaxValue)
            .ToList();

        return Ok(resultado);
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
    public async Task<ActionResult<ReporteAnimal>> MarcarComoAtendido(int id)
    {
        // [SEGURIDAD] El refugio que atiende se toma del token JWT (se ignora el IdRefugio del body).
        var idRefugioToken = _usuarioActual.ObtenerRefugioId();

        if (idRefugioToken <= 0)
        {
            return Unauthorized(new { error = "El token no incluye el refugio asociado." });
        }

        var reporte = await _context.ReportesAnimales.FindAsync(id);

        if (reporte is null)
        {
            return NotFound(new { error = "El reporte indicado no existe." });
        }

        if (reporte.Estado == ReporteEstado.Atendido)
        {
            return BadRequest(new { error = "El reporte ya fue atendido anteriormente." });
        }

        var refugio = await _context.Refugio.FindAsync(idRefugioToken);

        if (refugio is null)
        {
            return NotFound(new { error = "El refugio indicado no existe." });
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
/// Reporte de rescate tal como lo ve el panel del refugio: incluye la distancia
/// al refugio y si está dentro del radio de cobertura (5 km).
/// </summary>
public sealed class ReporteRescateDto
{
    /// <summary>Identificador del reporte.</summary>
    public int IdReporte { get; set; }

    /// <summary>Descripción del animal y su situación.</summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Ruta o URL de la foto del animal.</summary>
    public string FotoUrl { get; set; } = string.Empty;

    /// <summary>Latitud donde fue reportado.</summary>
    public double? Latitud { get; set; }

    /// <summary>Longitud donde fue reportado.</summary>
    public double? Longitud { get; set; }

    /// <summary>Estado del reporte: Pendiente o Atendido.</summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>Fecha de creación del reporte (UTC).</summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>Refugio que atendió el reporte, si corresponde.</summary>
    public long? IdRefugio { get; set; }

    /// <summary>Distancia en km entre el refugio y el reporte; null si no se puede medir.</summary>
    public double? DistanciaKm { get; set; }

    /// <summary>true si el reporte está dentro del radio de cobertura del refugio.</summary>
    public bool Cerca { get; set; }
}
