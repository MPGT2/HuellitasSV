using System.ComponentModel.DataAnnotations;

namespace HuellitasSV.API.Models;

/// <summary>
/// Estados posibles de una solicitud de adopción. Se almacenan como texto en la base de datos.
/// </summary>
public enum SolicitudEstado
{
    /// <summary>Solicitud creada, esperando decisión del refugio.</summary>
    Pendiente,

    /// <summary>Solicitud aprobada por el refugio.</summary>
    Aprobada,

    /// <summary>Solicitud rechazada por el refugio (también se usa cuando la mascota ya no está disponible).</summary>
    Rechazada
}

/// <summary>
/// Solicitud de adopción enviada por un usuario para una mascota publicada por un refugio.
/// </summary>
public class SolicitudAdopcion
{
    /// <summary>
    /// Identificador único de la solicitud.
    /// </summary>
    public int IdSolicitud { get; set; }

    /// <summary>
    /// Identificador de la mascota que se desea adoptar (clave de la tabla mascota).
    /// </summary>
    public long IdMascota { get; set; }

    /// <summary>
    /// Identificador del usuario solicitante (adoptante).
    /// </summary>
    public int IdUsuario { get; set; }

    /// <summary>
    /// Nombre de contacto confirmado por el usuario al enviar la solicitud.
    /// </summary>
    [Required(ErrorMessage = "El nombre de contacto es obligatorio.")]
    [StringLength(100)]
    public string NombreContacto { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto confirmado por el usuario.
    /// </summary>
    [Required(ErrorMessage = "El teléfono de contacto es obligatorio.")]
    [StringLength(20)]
    public string TelefonoContacto { get; set; } = string.Empty;

    /// <summary>
    /// Correo de contacto confirmado por el usuario.
    /// </summary>
    [Required(ErrorMessage = "El correo de contacto es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo de contacto no tiene un formato válido.")]
    [StringLength(150)]
    public string CorreoContacto { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual de la solicitud. Inicia en "Pendiente".
    /// </summary>
    public SolicitudEstado Estado { get; set; } = SolicitudEstado.Pendiente;

    /// <summary>
    /// Comentario del refugio al aprobar o rechazar la solicitud.
    /// </summary>
    public string? ComentarioDecision { get; set; }

    /// <summary>
    /// Fecha y hora de envío de la solicitud en UTC.
    /// </summary>
    public DateTime FechaSolicitud { get; set; }

    /// <summary>
    /// Mascota por la que se solicita la adopción.
    /// </summary>
    public Mascota? Mascota { get; set; }

    /// <summary>
    /// Usuario que envió la solicitud.
    /// </summary>
    public Usuario? Usuario { get; set; }
}
