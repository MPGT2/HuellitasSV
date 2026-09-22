namespace HuellitasSV.API.Models;

/// <summary>
/// Necesidad de donación publicada por un refugio (alimento, medicinas, dinero, etc.).
/// </summary>
public class NecesidadDonacion
{
    /// <summary>
    /// Identificador único de la necesidad de donación.
    /// </summary>
    public int IdNecesidad { get; set; }

    /// <summary>
    /// Identificador del refugio que publicó la necesidad.
    /// </summary>
    public int IdRefugio { get; set; }

    /// <summary>
    /// Título corto de la necesidad (ej.: "20 sacos de alimento para perros").
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de la necesidad.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Cantidad o monto requerido (precisión 18,2).
    /// </summary>
    public decimal CantidadRequerida { get; set; }

    /// <summary>
    /// Cantidad o monto ya cubierto por donaciones (precisión 18,2).
    /// </summary>
    public decimal CantidadCubierta { get; set; }

    /// <summary>
    /// Fecha y hora de publicación en UTC.
    /// </summary>
    public DateTime FechaPublicacion { get; set; }

    /// <summary>
    /// Refugio que publicó la necesidad de donación.
    /// </summary>
    public Refugio? Refugio { get; set; }
}
