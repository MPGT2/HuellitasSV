// [HU-XX] <Tu nombre>: Gestión y cobro de espacios publicitarios a tiendas (administrador).
// Ciclo de vida: "pendiente" -> "activo" (aprobado por el admin) -> "vencido" (al llegar fecha_fin).
// La visibilidad pública exige además pago_confirmado = true y que fecha_inicio ya haya iniciado.

namespace HuellitasSV.API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Espacio publicitario solicitado por una tienda y gestionado por el administrador
/// de la plataforma (aprobación, cobro y expiración automática).
/// </summary>
[Table("anuncio")]
public class Anuncio
{
    /// <summary>Identificador único (identity).</summary>
    [Key]
    [Column("id_anuncio")]
    public long IdAnuncio { get; set; }

    /// <summary>Nombre de la tienda que solicita el espacio publicitario.</summary>
    [Required]
    [MaxLength(100)]
    [Column("nombre_tienda")]
    public string NombreTienda { get; set; } = string.Empty;

    /// <summary>Correo o teléfono de contacto de la tienda (opcional).</summary>
    [MaxLength(150)]
    [Column("contacto_tienda")]
    public string? ContactoTienda { get; set; }

    /// <summary>URL de la imagen/banner del anuncio (opcional).</summary>
    [MaxLength(255)]
    [Column("imagen_url")]
    public string? ImagenUrl { get; set; }

    /// <summary>Descripción breve del anuncio (opcional).</summary>
    [MaxLength(255)]
    [Column("descripcion")]
    public string? Descripcion { get; set; }

    /// <summary>Monto cobrado por el espacio publicitario.</summary>
    [Required]
    [Column("precio", TypeName = "decimal(10,2)")]
    public decimal Precio { get; set; }

    /// <summary>Fecha en que el anuncio debe empezar a mostrarse.</summary>
    [Required]
    [Column("fecha_inicio")]
    public DateTime FechaInicio { get; set; }

    /// <summary>Fecha en que el anuncio deja de mostrarse (vence).</summary>
    [Required]
    [Column("fecha_fin")]
    public DateTime FechaFin { get; set; }

    /// <summary>Indica si el pago del espacio publicitario ya fue confirmado.</summary>
    [Required]
    [Column("pago_confirmado")]
    public bool PagoConfirmado { get; set; } = false;

    /// <summary>Estado: "pendiente", "activo", "rechazado" o "vencido".</summary>
    [Required]
    [MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "pendiente";

    /// <summary>Fecha en que el administrador aprobó el anuncio (nula si aún no se aprueba).</summary>
    [Column("fecha_aprobacion")]
    public DateTime? FechaAprobacion { get; set; }

    /// <summary>Fecha en que la tienda envió la solicitud (UTC).</summary>
    [Required]
    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}