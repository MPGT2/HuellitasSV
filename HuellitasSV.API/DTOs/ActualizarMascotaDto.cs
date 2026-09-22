// [HU-10] Michael Menendez: Estructura base - Contrato de entrada (DTO) para la actualización de mascotas.
// Todos los campos son opcionales para permitir actualizaciones parciales (PATCH-like con PUT).

namespace HuellitasSV.API.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Datos para actualizar una mascota; solo se modifican los campos enviados.
/// </summary>
public class ActualizarMascotaDto
{
    /// <summary>Nombre de la mascota.</summary>
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Especie: perro, gato u otro.</summary>
    [RegularExpression("^(?i)(perro|gato|otro)$", ErrorMessage = "Especie inválida. Valores permitidos: perro, gato, otro.")]
    public string? Especie { get; set; }

    /// <summary>Tamaño: pequeño, mediano o grande.</summary>
    [RegularExpression("^(?i)(pequeño|mediano|grande)$", ErrorMessage = "Tamaño inválido. Valores permitidos: pequeño, mediano, grande.")]
    public string? Tamano { get; set; }

    /// <summary>Edad en meses, entre 1 y 300.</summary>
    [Range(1, 300, ErrorMessage = "EdadMeses debe estar entre 1 y 300.")]
    public int? EdadMeses { get; set; }

    /// <summary>Estado de salud: sano, en_tratamiento, discapacidad o crónico.</summary>
    [RegularExpression("^(?i)(sano|en_tratamiento|discapacidad|crónico)$", ErrorMessage = "Estado de salud inválido. Valores permitidos: sano, en_tratamiento, discapacidad, crónico.")]
    public string? EstadoSalud { get; set; }

    /// <summary>Estado: disponible, adoptada, fallecida, en_tratamiento o reservada.</summary>
    [RegularExpression("^(?i)(disponible|adoptada|fallecida|en_tratamiento|reservada)$", ErrorMessage = "Estado inválido. Valores permitidos: disponible, adoptada, fallecida, en_tratamiento, reservada.")]
    public string? Estado { get; set; }

    /// <summary>Justificación exigida por la regla de negocio al pasar de 'adoptada' a 'disponible'.</summary>
    public string? JustificacionCambioEstado { get; set; }
}