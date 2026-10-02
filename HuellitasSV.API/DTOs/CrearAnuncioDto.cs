namespace HuellitasSV.API.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Datos requeridos para que una tienda solicite publicar un anuncio.
/// </summary>
public class CrearAnuncioDto
{
    /// <summary>Nombre de la tienda que solicita el espacio.</summary>
    [Required(ErrorMessage = "NombreTienda es obligatorio.")]
    [MaxLength(100, ErrorMessage = "NombreTienda no puede exceder 100 caracteres.")]
    public string NombreTienda { get; set; } = string.Empty;

    /// <summary>Correo o teléfono de contacto de la tienda (opcional).</summary>
    [MaxLength(150, ErrorMessage = "ContactoTienda no puede exceder 150 caracteres.")]
    public string? ContactoTienda { get; set; }

    /// <summary>Latitud de la tienda (opcional, permite calcular cercanía a un refugio).</summary>
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    public double? Latitud { get; set; }

    /// <summary>Longitud de la tienda (opcional, permite calcular cercanía a un refugio).</summary>
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    public double? Longitud { get; set; }

    /// <summary>URL de la imagen/banner del anuncio (opcional).</summary>
    [MaxLength(255, ErrorMessage = "ImagenUrl no puede exceder 255 caracteres.")]
    public string? ImagenUrl { get; set; }

    /// <summary>Descripción breve del anuncio (opcional).</summary>
    [MaxLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>Monto a cobrar por el espacio publicitario.</summary>
    [Required(ErrorMessage = "Precio es obligatorio.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    /// <summary>Fecha en que el anuncio debe empezar a mostrarse.</summary>
    [Required(ErrorMessage = "FechaInicio es obligatoria.")]
    public DateTime FechaInicio { get; set; }

    /// <summary>Fecha en que el anuncio debe dejar de mostrarse.</summary>
    [Required(ErrorMessage = "FechaFin es obligatoria.")]
    public DateTime FechaFin { get; set; }
}

