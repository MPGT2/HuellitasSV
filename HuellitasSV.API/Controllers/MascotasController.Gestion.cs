// [HU-09] Michael Menendez: Registro y gestión de mascotas desde el panel del refugio.
// Endpoints: POST, PUT y DELETE /api/Mascotas, listado por refugio y carga/servido de imagen.

namespace HuellitasSV.API.Controllers;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasSV.API.DTOs;
using HuellitasSV.API.Models;

public partial class MascotasController
{
    /// <summary>
    /// Mascotas de un refugio (filtro opcional por estado), ordenadas por fecha de registro descendente.
    /// </summary>
    /// <remarks>Usado por el panel del refugio y como ubicación de retorno del POST.</remarks>
    /// <response code="200">Lista de mascotas del refugio.</response>
    [HttpGet("refugio/{idRefugio}")]
    public async Task<IActionResult> GetByRefugio(long idRefugio, [FromQuery] string? estado = null)
    {
        var query = _context.Mascota.Where(m => m.IdRefugio == idRefugio).AsQueryable();

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

    /// <summary>
    /// Registra una mascota con estado "disponible" por defecto.
    /// </summary>
    /// <param name="dto">Datos validados de la mascota.</param>
    /// <returns>La mascota creada con su identificador.</returns>
    /// <response code="201">Mascota creada.</response>
    /// <response code="400">Datos inválidos o el refugio no existe.</response>
    [HttpPost]
    public async Task<IActionResult> RegistrarMascota([FromBody] RegistrarMascotaDto dto)
    {
        // El refugio debe existir antes de asignarle una mascota.
        var refugioExiste = await _context.Refugio.AnyAsync(r => r.IdRefugio == dto.IdRefugio);
        if (!refugioExiste)
            return BadRequest(new { error = "El refugio especificado no existe." });

        var mascota = new Mascota
        {
            IdRefugio = dto.IdRefugio,
            Nombre = dto.Nombre ?? string.Empty,
            Especie = dto.Especie.ToLower(),
            Tamano = dto.Tamano.ToLower(),
            EdadMeses = dto.EdadMeses,
            EstadoSalud = dto.EstadoSalud.ToLower(),
            Estado = "disponible",
            FechaRegistro = DateTime.UtcNow
        };

        _context.Mascota.Add(mascota);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByRefugio), new { idRefugio = mascota.IdRefugio }, new
        {
            mascota.IdMascota,
            mascota.Nombre,
            mascota.Especie,
            mascota.Tamano,
            mascota.EdadMeses,
            mascota.EstadoSalud,
            mascota.Estado,
            mascota.FechaRegistro
        });
    }

    /// <summary>
    /// Actualiza parcialmente una mascota o aplica una transición de estado.
    /// </summary>
    /// <response code="200">Mascota actualizada.</response>
    /// <response code="400">Transición de estado no permitida.</response>
    /// <response code="404">Mascota no encontrada.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarMascota(long id, [FromBody] ActualizarMascotaDto dto)
    {
        var mascotaExistente = await _context.Mascota.FindAsync(id);
        if (mascotaExistente == null)
            return NotFound(new { error = "Mascota no encontrada." });

        if (!string.IsNullOrEmpty(dto.Estado)
            && !EsTransicionEstadoValida(mascotaExistente.Estado, dto.Estado.ToLower(), dto.JustificacionCambioEstado))
        {
            return BadRequest(new
            {
                error = "Transición de estado no permitida. Cambiar de 'adoptada' a 'disponible' requiere justificación. 'fallecida' es irreversible."
            });
        }

        // Actualización parcial: solo se modifican los campos enviados.
        if (!string.IsNullOrEmpty(dto.Nombre))
            mascotaExistente.Nombre = dto.Nombre;
        if (!string.IsNullOrEmpty(dto.Especie))
            mascotaExistente.Especie = dto.Especie.ToLower();
        if (!string.IsNullOrEmpty(dto.Tamano))
            mascotaExistente.Tamano = dto.Tamano.ToLower();
        if (dto.EdadMeses.HasValue)
            mascotaExistente.EdadMeses = dto.EdadMeses.Value;
        if (!string.IsNullOrEmpty(dto.EstadoSalud))
            mascotaExistente.EstadoSalud = dto.EstadoSalud.ToLower();
        if (!string.IsNullOrEmpty(dto.Estado))
            mascotaExistente.Estado = dto.Estado.ToLower();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Mascota actualizada correctamente.",
            mascota = new
            {
                mascotaExistente.IdMascota,
                mascotaExistente.Nombre,
                mascotaExistente.Especie,
                mascotaExistente.Tamano,
                mascotaExistente.EdadMeses,
                mascotaExistente.EstadoSalud,
                mascotaExistente.Estado,
                mascotaExistente.FechaRegistro,
                mascotaExistente.ImagenUrl
            }
        });
    }

    /// <summary>
    /// Sube la imagen de una mascota (multipart/form-data, JPG/PNG/WebP, máx. 5 MB).
    /// La imagen se persiste como BLOB en la base de datos.
    /// </summary>
    /// <response code="200">Imagen subida.</response>
    /// <response code="400">Archivo o formato no válido.</response>
    /// <response code="404">Mascota no encontrada.</response>
    [HttpPost("{id}/imagen")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> SubirImagen(long id, IFormFile imagen)
    {
        var mascota = await _context.Mascota.FindAsync(id);
        if (mascota == null)
            return NotFound(new { error = "Mascota no encontrada." });

        if (imagen == null || imagen.Length == 0)
            return BadRequest(new { error = "No se recibió ninguna imagen." });

        // Validación de tipo MIME y tamaño máximo de 5 MB.
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(imagen.ContentType.ToLower()))
            return BadRequest(new { error = "Formato no permitido. Use JPG, PNG o WebP." });

        if (imagen.Length > 5_000_000)
            return BadRequest(new { error = "La imagen no debe exceder 5MB." });

        using var ms = new MemoryStream();
        await imagen.CopyToAsync(ms);
        mascota.ImagenData = ms.ToArray();
        mascota.ImagenContentType = imagen.ContentType;
        mascota.ImagenUrl = null;

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Imagen subida correctamente.", idMascota = mascota.IdMascota });
    }

    /// <summary>
    /// Sirve la imagen de una mascota: desde el BLOB almacenado o redirigiendo a URL externa.
    /// </summary>
    /// <response code="200">Archivo de imagen.</response>
    /// <response code="302">Redirección a URL externa.</response>
    /// <response code="404">Mascota sin imagen o no encontrada.</response>
    [HttpGet("{id}/imagen")]
    public async Task<IActionResult> ObtenerImagen(long id)
    {
        var mascota = await _context.Mascota.FindAsync(id);
        if (mascota == null)
            return NotFound(new { error = "Mascota no encontrada." });

        if (mascota.ImagenData != null && !string.IsNullOrEmpty(mascota.ImagenContentType))
            return File(mascota.ImagenData, mascota.ImagenContentType);

        if (!string.IsNullOrEmpty(mascota.ImagenUrl))
            return Redirect(mascota.ImagenUrl);

        return NotFound(new { error = "La mascota no tiene imagen." });
    }

    /// <summary>
    /// Elimina físicamente una mascota del sistema.
    /// </summary>
    /// <response code="200">Mascota eliminada.</response>
    /// <response code="404">Mascota no encontrada.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarMascota(long id)
    {
        var mascota = await _context.Mascota.FindAsync(id);
        if (mascota == null)
            return NotFound(new { error = "Mascota no encontrada." });

        _context.Mascota.Remove(mascota);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Mascota eliminada del sistema." });
    }
}