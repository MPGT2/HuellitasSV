// [HU-04] Michael Menendez: Catálogo público - Obtener mascotas disponibles por especie.
// Endpoint: GET /api/Mascotas/especie/{especie}

namespace HuellitasSV.API.Controllers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public partial class MascotasController
{
    /// <summary>
    /// Devuelve las mascotas en estado "disponible" de la especie indicada.
    /// </summary>
    /// <param name="especie">Especie a filtrar: perro, gato u otro.</param>
    /// <returns>Lista de mascotas disponibles.</returns>
    /// <response code="200">Lista de mascotas (puede estar vacía).</response>
    [HttpGet("especie/{especie}")]
    public async Task<IActionResult> ObtenerPorEspecie(string especie)
    {
        // Solo catálogo público: se excluyen mascotas no disponibles.
        var resultados = await _context.Mascota
            .Where(m => m.Especie.ToLower() == especie.ToLower() && m.Estado == "disponible")
            .Select(m => new { m.IdMascota, m.Nombre, m.Especie, m.Tamano, m.EdadMeses, m.EstadoSalud, m.Estado, m.FechaRegistro, m.ImagenUrl })
            .ToListAsync();

        return Ok(resultados.Any()
            ? resultados
            : new { mensaje = "Sin resultados para la especie seleccionada", datos = new List<object>() });
    }
}