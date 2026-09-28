namespace HuellitasSV.API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidad que representa una cuenta de acceso en HuellitasSV.
/// </summary>
[Table("cuenta")]
public class Cuenta
{
    /// <summary>Identificador único (identity).</summary>
    [Key]
    [Column("id_cuenta")]
    public long IdCuenta { get; set; }

    /// <summary>Correo de la cuenta (único en el sistema).</summary>
    [Required]
    [MaxLength(150)]
    [Column("correo")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Contraseña de la cuenta (almacenada con hash PBKDF2).</summary>
    [Required]
    [MaxLength(255)]
    [Column("contrasena")]
    public string Contrasena { get; set; } = string.Empty;

    /// <summary>Rol de la cuenta: Refugio o Admin.</summary>
    [Required]
    [MaxLength(20)]
    [Column("rol")]
    public string Rol { get; set; } = "Refugio";

    /// <summary>Estado de la cuenta: pendiente, aprobado o rechazado.</summary>
    [Required]
    [MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "pendiente";
}