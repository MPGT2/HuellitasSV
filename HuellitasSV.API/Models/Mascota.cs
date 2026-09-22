namespace HuellitasSV.API.Models;

/// <summary>
/// Estados de disponibilidad de una mascota. Se almacenan como texto en la base de datos.
/// </summary>
public enum MascotaEstado
{
    /// <summary>La mascota está disponible y puede recibir solicitudes de adopción.</summary>
    Disponible,

    /// <summary>La mascota tiene una adopción aprobada en trámite.</summary>
    EnProcesoAdopcion,

    /// <summary>La adopción se concretó; la mascota ya no está disponible.</summary>
    Adoptada
}

/// <summary>
/// Mascota publicada por un refugio para adopción o rescate.
/// </summary>
public class Mascota
{
    /// <summary>
    /// Identificador único de la mascota.
    /// </summary>
    public int IdMascota { get; set; }

    /// <summary>
    /// Identificador del refugio que publicó la mascota.
    /// </summary>
    public int IdRefugio { get; set; }

    /// <summary>
    /// Nombre de la mascota.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Especie de la mascota (Perro, Gato, etc.).
    /// </summary>
    public string Especie { get; set; } = string.Empty;

    /// <summary>
    /// Raza de la mascota, si se conoce.
    /// </summary>
    public string? Raza { get; set; }

    /// <summary>
    /// Sexo de la mascota (Macho, Hembra).
    /// </summary>
    public string? Sexo { get; set; }

    /// <summary>
    /// Edad aproximada de la mascota en meses.
    /// </summary>
    public int? EdadMeses { get; set; }

    /// <summary>
    /// Descripción de la mascota (características, temperamento, historia, etc.).
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// URL o ruta de la foto de la mascota.
    /// </summary>
    public string? FotoUrl { get; set; }

    /// <summary>
    /// Estado de disponibilidad de la mascota para adopción.
    /// </summary>
    public MascotaEstado Estado { get; set; } = MascotaEstado.Disponible;

    /// <summary>
    /// Fecha y hora de publicación en UTC.
    /// </summary>
    public DateTime FechaPublicacion { get; set; }

    /// <summary>
    /// Refugio al que pertenece la mascota.
    /// </summary>
    public Refugio? Refugio { get; set; }
}
