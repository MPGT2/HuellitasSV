// [HU-XX] <Tu nombre>: Contrato de entrada (DTO) para registrar un aporte de la comunidad.

namespace HuellitasSV.API.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Cantidad aportada por un donante hacia una necesidad de donación activa.
/// </summary>
public class RegistrarAporteDto
{
    /// <summary>Cantidad aportada; debe ser mayor a 0.</summary>
    [Required(ErrorMessage = "Cantidad es obligatoria.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Cantidad debe ser mayor a 0.")]
    public decimal Cantidad { get; set; }
}