using System.Text.Json.Serialization;

namespace HuellitasSV.API.Models;

/// <summary>
/// Usuario registrado en el sistema HuellitasSV (adoptantes, representantes de refugios, etc.).
/// </summary>
public class Usuario
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public int IdUsuario { get; set; }

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario. Es único en todo el sistema.
    /// </summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>
    /// Número telefónico de contacto del usuario.
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Contraseña de acceso del usuario. Nunca se incluye en las respuestas de la API.
    /// </summary>
    [JsonIgnore]
    public string Contrasena { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de registro en UTC. Se asigna automáticamente al crear el usuario.
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Refugio asociado cuando el usuario representa a un refugio. Un usuario tiene como máximo un refugio.
    /// </summary>
    public Refugio? Refugio { get; set; }
}
