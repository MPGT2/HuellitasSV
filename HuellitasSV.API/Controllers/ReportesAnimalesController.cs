using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using HuellitasSV.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para la creación de reportes de animales callejeros por parte de los usuarios (HU-14).
/// [SEGURIDAD] Solo accesible con token JWT de rol "Usuario"; el usuario se toma del token.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Usuario")]
public class ReportesAnimalesController : ControllerBase
{
    /// <summary>
    /// Radio de notificación en kilómetros: los refugios a esta distancia o menos reciben la alerta.
    /// </summary>
    private const double RadioNotificacionKm = 5.0;

    private readonly ApplicationDbContext _context;

    /// <summary>Claims del token ya convertidos.</summary>
    private readonly ICurrentUserService _usuarioActual;

    /// <summary>Servicio de guardado de la foto del reporte.</summary>
    private readonly IArchivoService _archivos;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ReportesAnimalesController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    /// <param name="usuarioActual">Claims del token ya convertidos.</param>
    /// <param name="archivos">Servicio de guardado de archivos.</param>
    public ReportesAnimalesController(
        ApplicationDbContext context,
        ICurrentUserService usuarioActual,
        IArchivoService archivos)
    {
        _context = context;
        _usuarioActual = usuarioActual;
        _archivos = archivos;
    }

    /// <summary>
    /// Obtiene todos los reportes de animal callejero del usuario autenticado.
    /// </summary>
    /// <param name="estado">Filtro opcional por estado: Pendiente, Atendido.</param>
    /// <returns>Lista de reportes del usuario ordenada por fecha descendente.</returns>
    /// <response code="200">Lista de reportes del usuario.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReporteAnimal>>> GetMisReportes([FromQuery] string? estado)
    {
        var idUsuarioToken = _usuarioActual.ObtenerUsuarioId();

        if (idUsuarioToken <= 0)
            return Unauthorized(new { error = "El token no incluye el perfil de usuario asociado." });

        var query = _context.ReportesAnimales
            .Where(r => r.IdUsuario == idUsuarioToken)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (Enum.TryParse<ReporteEstado>(estado, ignoreCase: true, out var estadoEnum))
            {
                query = query.Where(r => r.Estado == estadoEnum);
            }
        }

        var reportes = await query
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync();

        return Ok(reportes);
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
    /// <param name="dto">Datos del reporte (multipart/form-data): descripción, ubicación y foto.</param>
    /// <returns>El reporte creado con estado "Pendiente", o un error de validación.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReporteAnimal>> PostReporte([FromForm] CrearReporteDto dto)
    {
        // La validación de [Required]/[Range] del DTO bloquea aquí con 400 si faltan datos.

        // [SEGURIDAD] El usuario reportante se toma del token JWT (se ignora cualquier IdUsuario del cliente).
        var idUsuarioToken = _usuarioActual.ObtenerUsuarioId();

        if (idUsuarioToken <= 0)
        {
            return Unauthorized(new { error = "El token no incluye el perfil de usuario asociado." });
        }

        if (!await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuarioToken))
        {
            return NotFound(new { error = "El usuario indicado no existe." });
        }

        // La foto puede venir como archivo subido o, si no, como URL externa (retrocompatible).
        string? fotoUrl;
        try
        {
            fotoUrl = await _archivos.GuardarAsync(dto.Foto, "reportes") ?? dto.FotoUrl;
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        if (string.IsNullOrWhiteSpace(fotoUrl))
        {
            return BadRequest(new { error = "La foto del animal es obligatoria (sube un archivo o envía FotoUrl)." });
        }

        var reporte = new ReporteAnimal
        {
            IdUsuario = idUsuarioToken,
            Descripcion = dto.Descripcion,
            FotoUrl = fotoUrl!,
            Latitud = dto.Latitud,
            Longitud = dto.Longitud,
            Estado = ReporteEstado.Pendiente,
            FechaRegistro = DateTime.UtcNow
        };

        _context.ReportesAnimales.Add(reporte);

        // Notificación a los refugios cercanos a la ubicación del reporte.
        var refugiosConUbicacion = await _context.Refugio
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
                    Mensaje = $"Animal callejero reportado a {distancia:F1} km de tu refugio \"{refugio.NombreOrganizacion}\". Revisa el panel de rescates.",
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

/// <summary>
/// Datos para crear un reporte de animal callejero (HU-14) por multipart/form-data.
/// La foto se recibe como archivo (campo "Foto"); si no se envía, se admite una URL en "FotoUrl".
/// </summary>
public class CrearReporteDto
{
    /// <summary>Descripción del animal y su situación.</summary>
    [Required(ErrorMessage = "La descripción del animal es obligatoria.")]
    [StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Latitud de donde fue visto el animal. Obligatoria.</summary>
    [Required(ErrorMessage = "La ubicación es obligatoria: indica la latitud del animal reportado.")]
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    public double? Latitud { get; set; }

    /// <summary>Longitud de donde fue visto el animal. Obligatoria.</summary>
    [Required(ErrorMessage = "La ubicación es obligatoria: indica la longitud del animal reportado.")]
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    public double? Longitud { get; set; }

    /// <summary>URL externa de la foto (opcional; se ignora si se sube el archivo "Foto").</summary>
    [StringLength(500)]
    public string? FotoUrl { get; set; }

    /// <summary>Archivo de la foto del animal (opcional si se envía FotoUrl).</summary>
    public IFormFile? Foto { get; set; }
}
