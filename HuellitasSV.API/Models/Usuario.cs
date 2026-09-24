// [HU-01] Michael Menendez: Entidad Usuario (tabla usuario).
// Perfil de cliente de la plataforma: la autenticación vive en la tabla cuenta (correo/contraseña/rol)
// y este perfil guarda los datos personales. El estado de la cuenta (activo/inactivo/bloqueado)
// lo gestiona el administrador sobre la cuenta asociada.

namespace HuellitasSV.API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidad que representa el perfil de un usuario cliente de HuellitasSV.
/// </summary>
[Table("usuario")]
public class Usuario
{
    /// <summary>Identificador único (identity).</summary>
    [Key]
    [Column("id_usuario")]
    public long IdUsuario { get; set; }

    /// <summary>Cuenta de acceso asociada al usuario (correo, contraseña, rol y estado).</summary>
    [Required]
    [Column("id_cuenta")]
    public long IdCuenta { get; set; }

    /// <summary>Nombre completo del usuario.</summary>
    [Required]
    [MaxLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;
}