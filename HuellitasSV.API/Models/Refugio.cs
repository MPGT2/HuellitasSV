using System.ComponentModel.DataAnnotations;

namespace HuellitasSV.API.Models;

/// <summary>
/// Refugio de animales registrado en el sistema, asociado a un único usuario.
/// </summary>
public class Refugio
{
    /// <summary>
    /// Identificador único del refugio.
    /// </summary>
    public int IdRefugio { get; set; }

    /// <summary>
    /// Identificador del usuario dueño del refugio. Es una relación uno a uno.
    /// </summary>
    public int IdUsuario { get; set; }

    /// <summary>
    /// Nombre del refugio.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Dirección física del refugio.
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Número telefónico de contacto del refugio.
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Descripción general del refugio y su labor.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Latitud geográfica del refugio. Permite notificar a los refugios cercanos de un reporte (HU-14).
    /// </summary>
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    public double? Latitud { get; set; }

    /// <summary>
    /// Longitud geográfica del refugio. Permite notificar a los refugios cercanos de un reporte (HU-14).
    /// </summary>
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    public double? Longitud { get; set; }

    /// <summary>
    /// Usuario dueño del refugio.
    /// </summary>
    public Usuario? Usuario { get; set; }

    /// <summary>
    /// Mascotas publicadas por este refugio.
    /// </summary>
    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();

    /// <summary>
    /// Necesidades de donación publicadas por este refugio.
    /// </summary>
    public ICollection<NecesidadDonacion> NecesidadesDonacion { get; set; } = new List<NecesidadDonacion>();
}
