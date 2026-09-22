namespace HuellitasSV.API.Models;

/// <summary>
/// Notificación enviada a un refugio o a un usuario sobre un reporte o una solicitud de adopción.
/// </summary>
public class Notificacion
{
    /// <summary>
    /// Identificador único de la notificación.
    /// </summary>
    public int IdNotificacion { get; set; }

    /// <summary>
    /// Mensaje de la notificación (ej.: "Nueva solicitud de adopción para Firulais").
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del refugio destinatario. Nulo si la notificación es para un usuario.
    /// </summary>
    public long? IdRefugio { get; set; }

    /// <summary>
    /// Identificador del usuario destinatario. Nulo si la notificación es para un refugio.
    /// </summary>
    public int? IdUsuario { get; set; }

    /// <summary>
    /// Indica si la notificación ya fue leída por su destinatario.
    /// </summary>
    public bool Leida { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la notificación en UTC.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Refugio destinatario de la notificación, si corresponde.
    /// </summary>
    public Refugio? Refugio { get; set; }

    /// <summary>
    /// Usuario destinatario de la notificación, si corresponde.
    /// </summary>
    public Usuario? Usuario { get; set; }
}
