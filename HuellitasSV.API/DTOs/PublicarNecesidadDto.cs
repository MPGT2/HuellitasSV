// [HU-XX] <Tu nombre>: Contrato de entrada (DTO) para publicar una necesidad urgente de insumos.
// Las reglas de validación se declaran con DataAnnotations y se aplican automáticamente por la API.

namespace HuellitasSV.API.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Datos requeridos para que un refugio publique una necesidad de donación.
/// </summary>
public class PublicarNecesidadDto
{
    /// <summary>Refugio que publica la necesidad; debe existir en la base de datos.</summary>
    [Required(ErrorMessage = "IdRefugio es obligatorio.")]
    [Range(1, long.MaxValue, ErrorMessage = "IdRefugio debe ser mayor a 0.")]
    public long IdRefugio { get; set; }

    /// <summary>Tipo de insumo: alimento, medicina, manta o accesorio.</summary>
    [Required(ErrorMessage = "TipoInsumo es obligatorio.")]
    [RegularExpression("^(?i)(alimento|medicina|manta|accesorio)$",
        ErrorMessage = "TipoInsumo inválido. Valores permitidos: alimento, medicina, manta, accesorio.")]
    public string TipoInsumo { get; set; } = string.Empty;

    /// <summary>Detalle adicional del insumo requerido (opcional).</summary>
    [MaxLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>Cantidad total requerida para cubrir la necesidad.</summary>
    [Required(ErrorMessage = "CantidadRequerida es obligatoria.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "CantidadRequerida debe ser mayor a 0.")]
    public decimal CantidadRequerida { get; set; }
}