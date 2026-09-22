using System.ComponentModel.DataAnnotations;

namespace HuellitasSV.API.Models;

/// <summary>
/// Estados posibles de un reporte de animal callejero. Se almacenan como texto en la base de datos.
/// </summary>
public enum ReporteEstado
{
    /// <summary>Reporte creado y aún no atendido por ningún refugio.</summary>
    Pendiente,

    /// <summary>El refugio confirmó que atendió el reporte.</summary>
    Atendido
}

/// <summary>
/// Reporte de un perro o gato en situación de calle, enviado por un usuario para alertar a los refugios cercanos (HU-14).
/// </summary>
public class ReporteAnimal
{
    /// <summary>
    /// Identificador único del reporte.
    /// </summary>
    public int IdReporte { get; set; }

    /// <summary>
    /// Identificador del usuario que envía el reporte.
    /// </summary>
    public int IdUsuario { get; set; }

    /// <summary>
    /// Identificador del refugio que atendió el reporte. Nulo mientras el reporte está pendiente.
    /// </summary>
    public long? IdRefugio { get; set; }

    /// <summary>
    /// Descripción del animal y su situación.
    /// </summary>
    [Required(ErrorMessage = "La descripción del animal es obligatoria.")]
    [StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// URL o ruta de la foto del animal reportado.
    /// </summary>
    [Required(ErrorMessage = "La foto del animal es obligatoria.")]
    [StringLength(500)]
    public string FotoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Latitud de donde fue visto el animal. Es obligatoria: sin ubicación no se puede alertar a los refugios cercanos.
    /// </summary>
    [Required(ErrorMessage = "La ubicación es obligatoria: indica la latitud del animal reportado.")]
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    public double? Latitud { get; set; }

    /// <summary>
    /// Longitud de donde fue visto el animal. Es obligatoria: sin ubicación no se puede alertar a los refugios cercanos.
    /// </summary>
    [Required(ErrorMessage = "La ubicación es obligatoria: indica la longitud del animal reportado.")]
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    public double? Longitud { get; set; }

    /// <summary>
    /// Estado actual del reporte. Inicia en "Pendiente".
    /// </summary>
    public ReporteEstado Estado { get; set; } = ReporteEstado.Pendiente;

    /// <summary>
    /// Fecha y hora de creación del reporte en UTC.
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Usuario que envió el reporte.
    /// </summary>
    public Usuario? Usuario { get; set; }

    /// <summary>
    /// Refugio que atendió el reporte, si corresponde.
    /// </summary>
    public Refugio? Refugio { get; set; }
}
