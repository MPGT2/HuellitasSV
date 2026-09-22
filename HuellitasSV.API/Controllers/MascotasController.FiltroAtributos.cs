// [HU-05] Michael Menendez: Filtro por atributos - Obtener mascotas disponibles según tamaño,
// edad máxima y estado de salud. Endpoint: GET /api/Mascotas/atributos

namespace HuellitasSV.API.Controllers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public partial class MascotasController
{
    /// <summary>
    /// Filtra mascotas disponibles por atributos físicos y de salud (todos opcionales y combinables).
    /// </summary>
    /// <param name="tamano">Tamaño: pequeño, mediano o grande.</param>
    /// <param name="edadMaxMeses">Edad máxima en meses.</param>
    /// <param name="estadoSalud">Estado de salud: sano, en_tratamiento, discapacidad, crónico.</param>
    /// <returns>Lista de mascotas que cumplen los criterios.</returns>
    /// <response code="200">Lista de mascotas filtradas (puede estar vacía).</response>
    [HttpGet("atributos")]
    public async Task<IActionResult> FiltrarPorAtributos(
        [FromQuery] string? tamano,
        [FromQuery] int? edadMaxMeses,
        [FromQuery] string? estadoSalud)
    {
        var query = _context.Mascota.Where(m => m.Estado == "disponible").AsQueryable();

        // Cada filtro se aplica solo si viene presente en la consulta.
        if (!string.IsNullOrEmpty(tamano))
            query = query.Where(m => m.Tamano.ToLower() == tamano.ToLower());

        if (edadMaxMeses.HasValue)
            query = query.Where(m => m.EdadMeses <= edadMaxMeses.Value);

        if (!string.IsNullOrEmpty(estadoSalud))
            query = query.Where(m => m.EstadoSalud.ToLower() == estadoSalud.ToLower());

        var resultados = await query
            .Select(m => new { m.IdMascota, m.Nombre, m.Especie, m.Tamano, m.EdadMeses, m.EstadoSalud, m.Estado, m.FechaRegistro, m.ImagenUrl })
            .ToListAsync();

        return Ok(resultados.Any()
            ? resultados
            : new { mensaje = "Sin resultados para los atributos seleccionados", datos = new List<object>() });
    }
}