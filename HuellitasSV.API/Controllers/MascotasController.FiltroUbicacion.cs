// [HU-06] Michael Menendez: Filtro por ubicación - Obtener mascotas disponibles según el
// departamento y municipio del refugio que las resguarda. Endpoint: GET /api/Mascotas/ubicacion

namespace HuellitasSV.API.Controllers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public partial class MascotasController
{
    /// <summary>
    /// Filtra mascotas disponibles por la ubicación del refugio (departamento y/o municipio).
    /// </summary>
    /// <param name="departamento">Departamento del refugio.</param>
    /// <param name="municipio">Municipio del refugio (requiere departamento).</param>
    /// <returns>Lista de mascotas filtradas por ubicación.</returns>
    /// <response code="200">Lista de mascotas filtradas.</response>
    /// <response code="400">Si se envía municipio sin departamento.</response>
    [HttpGet("ubicacion")]
    public async Task<IActionResult> FiltrarPorUbicacion(
        [FromQuery] string? departamento,
        [FromQuery] string? municipio)
    {
        // El municipio no puede filtrarse sin su departamento padre.
        if (!string.IsNullOrEmpty(municipio) && string.IsNullOrEmpty(departamento))
        {
            return BadRequest(new { error = "Debe seleccionar primero un departamento antes de aplicar el filtro por municipio." });
        }

        var query = _context.Mascota
            .Include(m => m.Refugio)
            .Where(m => m.Estado == "disponible")
            .AsQueryable();

        if (!string.IsNullOrEmpty(departamento))
            query = query.Where(m => m.Refugio!.Departamento.ToLower() == departamento.ToLower());

        if (!string.IsNullOrEmpty(municipio))
            query = query.Where(m => m.Refugio!.Municipio.ToLower() == municipio.ToLower());

        var resultados = await query
            .Select(m => new
            {
                m.IdMascota,
                m.Nombre,
                m.Especie,
                m.Tamano,
                m.EdadMeses,
                m.EstadoSalud,
                m.Estado,
                m.FechaRegistro,
                m.ImagenUrl,
                Refugio = new { m.Refugio!.NombreOrganizacion, m.Refugio.Departamento, m.Refugio.Municipio }
            })
            .ToListAsync();

        return Ok(resultados.Any()
            ? resultados
            : new { mensaje = "Sin resultados para la ubicación seleccionada", datos = new List<object>() });
    }
}