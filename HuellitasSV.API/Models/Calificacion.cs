namespace HuellitasSV.API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Calificación por estrellas (1 a 5) que un usuario otorga a un refugio (HU-10).
/// Un usuario solo puede calificar una vez a cada refugio.
/// </summary>
[Table("calificacion")]
public class Calificacion
{
    /// <summary>Identificador único (identity).</summary>
    [Key]
    [Column("id_calificacion")]
    public long IdCalificacion { get; set; }

    /// <summary>Refugio calificado (FK hacia refugio).</summary>
    [Required]
    [Column("id_refugio")]
    public long IdRefugio { get; set; }

    /// <summary>Usuario que califica (FK hacia usuario).</summary>
    [Required]
    [Column("id_usuario")]
    public long IdUsuario { get; set; }

    /// <summary>Cantidad de estrellas otorgadas, entre 1 y 5.</summary>
    [Required]
    [Range(1, 5, ErrorMessage = "Las estrellas deben estar entre 1 y 5.")]
    [Column("estrellas")]
    public int Estrellas { get; set; }

    /// <summary>Comentario opcional del usuario.</summary>
    [MaxLength(500)]
    [Column("comentario")]
    public string? Comentario { get; set; }

    /// <summary>Fecha de la calificación (UTC).</summary>
    [Required]
    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    /// <summary>Navegación al refugio calificado.</summary>
    [ForeignKey(nameof(IdRefugio))]
    public Refugio? Refugio { get; set; }

    /// <summary>Navegación al usuario que calificó.</summary>
    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }
}