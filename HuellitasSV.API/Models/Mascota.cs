// [HU-10] Michael Menendez: Estructura base - Entidad Mascota (tabla mascota).
// Los valores permitidos de Especie, Tamano, EstadoSalud y Estado se normalizan en minúsculas.

namespace HuellitasSV.API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidad que representa una mascota en HuellitasSV.
/// </summary>
[Table("mascota")]
public class Mascota
{
    /// <summary>Identificador único (identity).</summary>
    [Key]
    [Column("id_mascota")]
    public long IdMascota { get; set; }

    /// <summary>Refugio al que pertenece (FK, borrado restrictivo).</summary>
    [Required]
    [Column("id_refugio")]
    public long IdRefugio { get; set; }

    /// <summary>Nombre de la mascota.</summary>
    [MaxLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Especie: perro, gato u otro.</summary>
    [Required]
    [MaxLength(20)]
    [Column("especie")]
    public string Especie { get; set; } = string.Empty;

    /// <summary>Tamaño: pequeño, mediano o grande.</summary>
    [Required]
    [MaxLength(20)]
    [Column("tamano")]
    public string Tamano { get; set; } = string.Empty;

    /// <summary>Edad expresada en meses.</summary>
    [Required]
    [Column("edad_meses")]
    public int EdadMeses { get; set; }

    /// <summary>Estado de salud: sano, en_tratamiento, discapacidad o crónico.</summary>
    [Required]
    [MaxLength(20)]
    [Column("estado_salud")]
    public string EstadoSalud { get; set; } = string.Empty;

    /// <summary>Estado: disponible, adoptada, fallecida, en_tratamiento o reservada.</summary>
    [Required]
    [MaxLength(30)]
    [Column("estado")]
    public string Estado { get; set; } = "disponible";

    /// <summary>Fecha de registro (UTC).</summary>
    [Required]
    [Column("fecha_registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>URL externa de la imagen (alternativa al BLOB).</summary>
    [MaxLength(500)]
    [Column("imagen_url")]
    public string? ImagenUrl { get; set; }

    /// <summary>Imagen almacenada como BLOB en la base de datos.</summary>
    [Column("imagen_data")]
    public byte[]? ImagenData { get; set; }

    /// <summary>Tipo MIME de la imagen almacenada (image/jpeg, image/png, etc.).</summary>
    [MaxLength(50)]
    [Column("imagen_content_type")]
    public string? ImagenContentType { get; set; }

    /// <summary>Navegación al refugio propietario.</summary>
    [ForeignKey(nameof(IdRefugio))]
    public Refugio? Refugio { get; set; }
}