// [HU-01] Michael Menendez: Entidad Usuario (tabla usuario).
// Perfil de un usuario registrado; sus credenciales viven en la cuenta asociada (id_cuenta).

namespace HuellitasSV.API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Perfil de un usuario registrado en el sistema HuellitasSV. Las credenciales de acceso
/// (correo y contraseña) se almacenan en la entidad Cuenta vinculada por IdCuenta.
/// </summary>
[Table("usuario")]
public class Usuario
{
    /// <summary>Identificador único (identity).</summary>
    [Key]
    [Column("id_usuario")]
    public long IdUsuario { get; set; }

    /// <summary>Cuenta de acceso asociada al perfil (FK lógica hacia cuenta).</summary>
    [Required]
    [Column("id_cuenta")]
    public long IdCuenta { get; set; }

    /// <summary>Nombre completo del usuario.</summary>
    [Required]
    [MaxLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;
}