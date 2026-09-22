// [HU-10] Michael Menendez: Estructura base - Contrato de entrada (DTO) para el registro de mascotas.
// Las reglas de validación se declaran con DataAnnotations y se aplican automáticamente por la API.

namespace HuellitasSV.API.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Datos requeridos para registrar una nueva mascota.
/// </summary>
public class RegistrarMascotaDto
{
    /// <summary>Refugio que registra la mascota; debe existir en la base de datos.</summary>
    [Required(ErrorMessage = "IdRefugio es obligatorio.")]
    [Range(1, long.MaxValue, ErrorMessage = "IdRefugio debe ser mayor a 0.")]
    public long IdRefugio { get; set; }

    /// <summary>Nombre de la mascota (opcional).</summary>
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Especie: perro, gato u otro.</summary>
    [Required(ErrorMessage = "Especie es obligatoria.")]
    [RegularExpression("^(?i)(perro|gato|otro)$", ErrorMessage = "Especie inválida. Valores permitidos: perro, gato, otro.")]
    public string Especie { get; set; } = string.Empty;

    /// <summary>Tamaño: pequeño, mediano o grande.</summary>
    [Required(ErrorMessage = "Tamaño es obligatorio.")]
    [RegularExpression("^(?i)(pequeño|mediano|grande)$", ErrorMessage = "Tamaño inválido. Valores permitidos: pequeño, mediano, grande.")]
    public string Tamano { get; set; } = string.Empty;

    /// <summary>Edad en meses, entre 1 y 300.</summary>
    [Required(ErrorMessage = "EdadMeses es obligatoria.")]
    [Range(1, 300, ErrorMessage = "EdadMeses debe estar entre 1 y 300.")]
    public int EdadMeses { get; set; }

    /// <summary>Estado de salud: sano, en_tratamiento, discapacidad o crónico.</summary>
    [Required(ErrorMessage = "Estado de salud es obligatorio.")]
    [RegularExpression("^(?i)(sano|en_tratamiento|discapacidad|crónico)$", ErrorMessage = "Estado de salud inválido. Valores permitidos: sano, en_tratamiento, discapacidad, crónico.")]
    public string EstadoSalud { get; set; } = string.Empty;
}