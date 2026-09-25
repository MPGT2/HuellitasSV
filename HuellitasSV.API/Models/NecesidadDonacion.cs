// [HU-XX] <Tu nombre>: Publicación de necesidad urgente de insumos por parte del refugio.
// Estado por defecto "activa"; pasa a "cubierta" automáticamente cuando cantidad_cubierta alcanza cantidad_requerida.

namespace HuellitasSV.API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidad que representa una necesidad urgente de insumos publicada por un refugio
/// (alimentos, medicinas, mantas, accesorios) para solicitar apoyo a la comunidad.
/// </summary>
[Table("necesidad_donacion")]
public class NecesidadDonacion
{
    /// <summary>Identificador único (identity).</summary>
    [Key]
    [Column("id_necesidad")]
    public long IdNecesidad { get; set; }

    /// <summary>Refugio que publica la necesidad (FK, borrado restrictivo).</summary>
    [Required]
    [Column("id_refugio")]
    public long IdRefugio { get; set; }

    /// <summary>Tipo de insumo: alimento, medicina, manta o accesorio.</summary>
    [Required]
    [MaxLength(20)]
    [Column("tipo_insumo")]
    public string TipoInsumo { get; set; } = string.Empty;

    /// <summary>Detalle adicional del insumo requerido (opcional).</summary>
    [MaxLength(255)]
    [Column("descripcion")]
    public string? Descripcion { get; set; }

    /// <summary>Cantidad total requerida para cubrir la necesidad.</summary>
    [Required]
    [Column("cantidad_requerida", TypeName = "decimal(10,2)")]
    public decimal CantidadRequerida { get; set; }

    /// <summary>Cantidad acumulada que ya ha sido aportada por la comunidad.</summary>
    [Required]
    [Column("cantidad_cubierta", TypeName = "decimal(10,2)")]
    public decimal CantidadCubierta { get; set; } = 0;

    /// <summary>Estado: "activa" mientras falte cantidad por cubrir, "cubierta" al alcanzar el total.</summary>
    [Required]
    [MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "activa";

    /// <summary>Fecha de publicación (UTC).</summary>
    [Required]
    [Column("fecha_publicacion")]
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

    /// <summary>Navegación al refugio que publicó la necesidad.</summary>
    [ForeignKey(nameof(IdRefugio))]
    public Refugio? Refugio { get; set; }
}