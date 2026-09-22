// [HU-10] Michael Menendez: Estructura base - Entidad Refugio (tabla refugio).
// El estado de aprobación (pendiente/aprobado/rechazado) lo gestiona el proceso de solicitud de refugios.

namespace HuellitasSV.API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidad que representa un refugio de animales en HuellitasSV.
/// </summary>
[Table("refugio")]
public class Refugio
{
    /// <summary>Identificador único (identity).</summary>
    [Key]
    [Column("id_refugio")]
    public long IdRefugio { get; set; }

    /// <summary>Cuenta de usuario asociada al refugio.</summary>
    [Required]
    [Column("id_cuenta")]
    public long IdCuenta { get; set; }

    /// <summary>Nombre de la organización.</summary>
    [Required]
    [MaxLength(150)]
    [Column("nombre_organizacion")]
    public string NombreOrganizacion { get; set; } = string.Empty;

    /// <summary>Departamento donde se ubica.</summary>
    [Required]
    [MaxLength(100)]
    [Column("departamento")]
    public string Departamento { get; set; } = string.Empty;

    /// <summary>Municipio donde se ubica.</summary>
    [Required]
    [MaxLength(100)]
    [Column("municipio")]
    public string Municipio { get; set; } = string.Empty;

    /// <summary>Información de contacto.</summary>
    [Required]
    [MaxLength(150)]
    [Column("contacto")]
    public string Contacto { get; set; } = string.Empty;

    /// <summary>URL de documentación de respaldo (opcional).</summary>
    [MaxLength(255)]
    [Column("documentacion_url")]
    public string? DocumentacionUrl { get; set; }

    /// <summary>Estado de aprobación: pendiente, aprobado o rechazado.</summary>
    [Required]
    [MaxLength(20)]
    [Column("estado_aprobacion")]
    public string EstadoAprobacion { get; set; } = "pendiente";
}