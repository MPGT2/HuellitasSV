// [HU-09] Michael Menendez: Panel del refugio - Consultar las mascotas asociadas a un refugio.
// Endpoint: GET /api/Refugios/{id}/mascotas

namespace HuellitasSV.API.Controllers;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;

/// <summary>
/// Controlador de consulta de mascotas por refugio (panel del refugio).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RefugiosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa el controlador con el contexto de base de datos.
    /// </summary>
    public RefugiosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Devuelve las mascotas de un refugio con filtro opcional por estado (por defecto "disponible").
    /// </summary>
    /// <param name="id">Identificador del refugio.</param>
    /// <param name="estado">Estado a filtrar: disponible, adoptada, fallecida, en_tratamiento, reservada.</param>
    /// <returns>Lista de mascotas ordenadas por fecha descendente.</returns>
    /// <response code="200">Lista de mascotas del refugio.</response>
    /// <response code="404">Refugio no encontrado.</response>
    [HttpGet("{id}/mascotas")]
    public async Task<IActionResult> GetMascotas(long id, [FromQuery] string? estado = "disponible")
    {
        var refugio = await _context.Refugio.FindAsync(id);
        if (refugio == null)
            return NotFound(new { error = "Refugio no encontrado." });

        var query = _context.Mascota.Where(m => m.IdRefugio == id).AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(m => m.Estado == estado);

        var mascotas = await query
            .OrderByDescending(m => m.FechaRegistro)
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
                TieneImagen = m.ImagenData != null || !string.IsNullOrEmpty(m.ImagenUrl)
            })
            .ToListAsync();

        return Ok(mascotas);
    }
}