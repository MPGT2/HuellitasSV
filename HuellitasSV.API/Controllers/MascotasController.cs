// [HU-04 | HU-05 | HU-06 | HU-09] Michael Menendez: Controlador de mascotas unificado en un único archivo.
// Este archivo consolida y reemplaza los antiguos partial: MascotasController.Gestion.cs,
// MascotasController.CatalogoEspecie.cs, MascotasController.FiltroAtributos.cs y MascotasController.FiltroUbicacion.cs.
// Orden jerárquico estricto: 1) Constructor e inyección de dependencias, 2) [HttpGet], 3) [HttpPost], 4) [HttpPut], 5) [HttpDelete].

namespace HuellitasSV.API.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuellitasSV.API.Data;
using HuellitasSV.API.DTOs;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Controlador de mascotas de HuellitasSV: catálogo público y gestión CRUD (HU-09),
/// filtro por especie (HU-04), filtro por atributos (HU-05) y filtro por ubicación del refugio (HU-06).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    /// <summary>Contexto de base de datos inyectado por el contenedor de dependencias.</summary>
    private readonly ApplicationDbContext _context;

    // ============================================================
    // 1) CONSTRUCTOR E INYECCIÓN DE DEPENDENCIAS
    // ============================================================

    /// <summary>
    /// Inicializa una nueva instancia del controlador de mascotas.
    /// </summary>
    /// <param name="context">Contexto de Entity Framework Core de HuellitasSV.</param>
    public MascotasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // 2) MÉTODOS [HttpGet]
    // ============================================================

    /// <summary>
    /// [HU-09] Obtiene el catálogo público con todas las mascotas en estado "disponible".
    /// </summary>
    /// <returns>ActionResult con la lista de mascotas disponibles en formato JSON.</returns>
    /// <response code="200">Catálogo de mascotas disponibles (lista vacía si no hay resultados).</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MascotaRespuestaDto>>> ObtenerCatalogo()
    {
        var catalogo = await _context.Mascota
            .Where(m => m.Estado == "disponible")
            .OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MascotaRespuestaDto
            {
                IdMascota = m.IdMascota,
                Nombre = m.Nombre,
                Especie = m.Especie,
                Tamano = m.Tamano,
                EdadMeses = m.EdadMeses,
                EstadoSalud = m.EstadoSalud,
                Estado = m.Estado,
                FechaRegistro = m.FechaRegistro,
                ImagenUrl = m.ImagenUrl
            })
            .ToListAsync();

        return Ok(catalogo);
    }

    /// <summary>
    /// [HU-09] Obtiene el detalle de una mascota por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la mascota.</param>
    /// <returns>ActionResult con la mascota solicitada en formato JSON.</returns>
    /// <response code="200">Mascota encontrada.</response>
    /// <response code="404">Mascota no encontrada.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<MascotaRespuestaDto>> ObtenerPorId(long id)
    {
        var mascota = await _context.Mascota
            .Where(m => m.IdMascota == id)
            .Select(m => new MascotaRespuestaDto
            {
                IdMascota = m.IdMascota,
                Nombre = m.Nombre,
                Especie = m.Especie,
                Tamano = m.Tamano,
                EdadMeses = m.EdadMeses,
                EstadoSalud = m.EstadoSalud,
                Estado = m.Estado,
                FechaRegistro = m.FechaRegistro,
                ImagenUrl = m.ImagenUrl
            })
            .FirstOrDefaultAsync();

        if (mascota is null)
            return NotFound(new { error = "Mascota no encontrada." });

        return Ok(mascota);
    }

    /// <summary>
    /// [HU-04] Obtiene únicamente las mascotas disponibles de la especie indicada.
    /// </summary>
    /// <param name="especie">Especie a filtrar: perro, gato u otro.</param>
    /// <returns>ActionResult con la lista de mascotas disponibles de la especie en formato JSON.</returns>
    /// <response code="200">Lista de mascotas de la especie (lista vacía si no hay resultados).</response>
    [HttpGet("especie/{especie}")]
    public async Task<ActionResult<IEnumerable<MascotaRespuestaDto>>> ObtenerPorEspecie(string especie)
    {
        var resultados = await _context.Mascota
            .Where(m => m.Especie.ToLower() == especie.ToLower() && m.Estado == "disponible")
            .OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MascotaRespuestaDto
            {
                IdMascota = m.IdMascota,
                Nombre = m.Nombre,
                Especie = m.Especie,
                Tamano = m.Tamano,
                EdadMeses = m.EdadMeses,
                EstadoSalud = m.EstadoSalud,
                Estado = m.Estado,
                FechaRegistro = m.FechaRegistro,
                ImagenUrl = m.ImagenUrl
            })
            .ToListAsync();

        return Ok(resultados);
    }

    /// <summary>
    /// [HU-05] Filtra mascotas disponibles por atributos físicos y de salud.
    /// Todos los filtros son opcionales y combinables entre sí.
    /// </summary>
    /// <param name="tamano">Tamaño opcional de la mascota: pequeño, mediano o grande.</param>
    /// <param name="edadMaxMeses">Edad máxima opcional expresada en meses.</param>
    /// <param name="estadoSalud">Estado de salud opcional: sano, en_tratamiento, discapacidad o crónico.</param>
    /// <returns>ActionResult con la lista de mascotas que cumplen los criterios en formato JSON.</returns>
    /// <response code="200">Lista de mascotas filtrada (lista vacía si no hay resultados).</response>
    [HttpGet("atributos")]
    public async Task<ActionResult<IEnumerable<MascotaRespuestaDto>>> FiltrarPorAtributos(
        [FromQuery] string? tamano,
        [FromQuery] int? edadMaxMeses,
        [FromQuery] string? estadoSalud)
    {
        var query = _context.Mascota.Where(m => m.Estado == "disponible").AsQueryable();

        if (!string.IsNullOrEmpty(tamano))
            query = query.Where(m => m.Tamano.ToLower() == tamano.ToLower());

        if (edadMaxMeses.HasValue)
            query = query.Where(m => m.EdadMeses <= edadMaxMeses.Value);

        if (!string.IsNullOrEmpty(estadoSalud))
            query = query.Where(m => m.EstadoSalud.ToLower() == estadoSalud.ToLower());

        var resultados = await query
            .OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MascotaRespuestaDto
            {
                IdMascota = m.IdMascota,
                Nombre = m.Nombre,
                Especie = m.Especie,
                Tamano = m.Tamano,
                EdadMeses = m.EdadMeses,
                EstadoSalud = m.EstadoSalud,
                Estado = m.Estado,
                FechaRegistro = m.FechaRegistro,
                ImagenUrl = m.ImagenUrl
            })
            .ToListAsync();

        return Ok(resultados);
    }

    /// <summary>
    /// [HU-06] Filtra mascotas disponibles por la ubicación del refugio que las resguarda,
    /// cruzando las tablas Mascota y Refugio mediante Include(m => m.Refugio).
    /// Regla de negocio: si se envía el municipio, el departamento es obligatorio.
    /// </summary>
    /// <param name="departamento">Departamento opcional del refugio.</param>
    /// <param name="municipio">Municipio opcional del refugio; requiere el departamento.</param>
    /// <returns>ActionResult con la lista de mascotas filtradas por ubicación en formato JSON.</returns>
    /// <response code="200">Lista de mascotas con los datos del refugio (lista vacía si no hay resultados).</response>
    /// <response code="400">Se envió el municipio sin el departamento.</response>
    [HttpGet("ubicacion")]
    public async Task<ActionResult<IEnumerable<MascotaRespuestaDto>>> FiltrarPorUbicacion(
        [FromQuery] string? departamento,
        [FromQuery] string? municipio)
    {
        if (!string.IsNullOrEmpty(municipio) && string.IsNullOrEmpty(departamento))
            return BadRequest(new { error = "Debe seleccionar primero un departamento antes de filtrar por municipio." });

        var query = _context.Mascota
            .Include(m => m.Refugio)
            .Where(m => m.Estado == "disponible")
            .AsQueryable();

        if (!string.IsNullOrEmpty(departamento))
            query = query.Where(m => m.Refugio!.Departamento.ToLower() == departamento.ToLower());

        if (!string.IsNullOrEmpty(municipio))
            query = query.Where(m => m.Refugio!.Municipio.ToLower() == municipio.ToLower());

        var resultados = await query
            .OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MascotaRespuestaDto
            {
                IdMascota = m.IdMascota,
                Nombre = m.Nombre,
                Especie = m.Especie,
                Tamano = m.Tamano,
                EdadMeses = m.EdadMeses,
                EstadoSalud = m.EstadoSalud,
                Estado = m.Estado,
                FechaRegistro = m.FechaRegistro,
                ImagenUrl = m.ImagenUrl,
                Refugio = new RefugioRespuestaDto
                {
                    NombreOrganizacion = m.Refugio!.NombreOrganizacion,
                    Departamento = m.Refugio!.Departamento,
                    Municipio = m.Refugio!.Municipio
                }
            })
            .ToListAsync();

        return Ok(resultados);
    }

    /// <summary>
    /// [HU-09] Obtiene las mascotas registradas por un refugio, con filtro opcional por estado.
    /// </summary>
    /// <param name="idRefugio">Identificador único del refugio.</param>
    /// <param name="estado">Estado opcional: disponible, adoptada, fallecida, en_tratamiento o reservada.</param>
    /// <returns>ActionResult con la lista de mascotas del refugio en formato JSON.</returns>
    /// <response code="200">Lista de mascotas del refugio ordenada por fecha de registro descendente.</response>
    [HttpGet("refugio/{idRefugio}")]
    public async Task<ActionResult<IEnumerable<MascotaRespuestaDto>>> ObtenerPorRefugio(
        long idRefugio,
        [FromQuery] string? estado = null)
    {
        var query = _context.Mascota.Where(m => m.IdRefugio == idRefugio).AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(m => m.Estado == estado);

        var mascotas = await query
            .OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MascotaRespuestaDto
            {
                IdMascota = m.IdMascota,
                Nombre = m.Nombre,
                Especie = m.Especie,
                Tamano = m.Tamano,
                EdadMeses = m.EdadMeses,
                EstadoSalud = m.EstadoSalud,
                Estado = m.Estado,
                FechaRegistro = m.FechaRegistro,
                ImagenUrl = m.ImagenUrl
            })
            .ToListAsync();

        return Ok(mascotas);
    }

    // ============================================================
    // 3) MÉTODOS [HttpPost]
    // ============================================================

    /// <summary>
    /// [HU-09] Registra una nueva mascota en estado "disponible" por defecto.
    /// Admite envío multipart/form-data para incluir una imagen de la mascota.
    /// </summary>
    /// <param name="dto">Datos validados de la mascota (IdRefugio, Nombre, Especie, Tamano, EdadMeses y EstadoSalud).</param>
    /// <param name="imagen">Archivo de imagen de la mascota (opcional, multipart/form-data).</param>
    /// <returns>ActionResult con la mascota creada en formato JSON y su URI de consulta.</returns>
    /// <response code="201">Mascota registrada correctamente.</response>
    /// <response code="400">Datos inválidos, refugio inexistente o error al procesar la imagen.</response>
    [HttpPost]
    public async Task<ActionResult<MascotaRespuestaDto>> RegistrarMascota([FromForm] RegistrarMascotaDto dto, IFormFile? imagen)
    {
        var refugioExiste = await _context.Refugio.AnyAsync(r => r.IdRefugio == dto.IdRefugio);
        if (!refugioExiste)
            return BadRequest(new { error = "El refugio especificado no existe." });

        // Procesar imagen si se proporciona
        string? urlImagen = null;
        if (imagen != null && imagen.Length > 0)
        {
            // Asegurar que la carpeta exista
            var carpetaImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", "mascotas");
            if (!Directory.Exists(carpetaImagenes))
                Directory.CreateDirectory(carpetaImagenes);

            // Generar nombre de archivo único
            var extension = Path.GetExtension(imagen.FileName);
            var nombreArchivo = $"mascota_{Guid.NewGuid()}{extension}";
            var rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);

            // Guardar el archivo
            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            urlImagen = $"/imagenes/mascotas/{nombreArchivo}";
        }

        var mascota = new Mascota
        {
            IdRefugio = dto.IdRefugio,
            Nombre = dto.Nombre ?? string.Empty,
            Especie = dto.Especie.ToLower(),
            Tamano = dto.Tamano.ToLower(),
            EdadMeses = dto.EdadMeses,
            EstadoSalud = dto.EstadoSalud.ToLower(),
            Estado = "disponible",
            FechaRegistro = DateTime.UtcNow,
            ImagenUrl = urlImagen
        };

        _context.Mascota.Add(mascota);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = mascota.IdMascota }, new MascotaRespuestaDto
        {
            IdMascota = mascota.IdMascota,
            Nombre = mascota.Nombre,
            Especie = mascota.Especie,
            Tamano = mascota.Tamano,
            EdadMeses = mascota.EdadMeses,
            EstadoSalud = mascota.EstadoSalud,
            Estado = mascota.Estado,
            FechaRegistro = mascota.FechaRegistro,
            ImagenUrl = mascota.ImagenUrl
        });
    }

    // ============================================================
    // 4) MÉTODOS [HttpPut]
    // ============================================================

    /// <summary>
    /// [HU-09] Actualiza parcialmente una mascota o aplica una transición de estado.
    /// Regla de negocio: pasar de "adoptada" a "disponible" exige justificación y "fallecida" es irreversible.
    /// </summary>
    /// <param name="id">Identificador único de la mascota a actualizar.</param>
    /// <param name="dto">Campos opcionales a modificar (Nombre, Especie, Tamano, EdadMeses, EstadoSalud, Estado y JustificacionCambioEstado).</param>
    /// <returns>ActionResult con la mascota actualizada en formato JSON.</returns>
    /// <response code="200">Mascota actualizada correctamente.</response>
    /// <response code="400">Transición de estado no permitida.</response>
    /// <response code="404">Mascota no encontrada.</response>
    [HttpPut("{id}")]
    public async Task<ActionResult<MascotaRespuestaDto>> ActualizarMascota(long id, [FromBody] ActualizarMascotaDto dto)
    {
        var mascotaExistente = await _context.Mascota.FindAsync(id);
        if (mascotaExistente is null)
            return NotFound(new { error = "Mascota no encontrada." });

        if (!string.IsNullOrEmpty(dto.Estado)
            && !EsTransicionEstadoValida(mascotaExistente.Estado, dto.Estado.ToLower(), dto.JustificacionCambioEstado))
        {
            return BadRequest(new
            {
                error = "Transición de estado no permitida. Cambiar de 'adoptada' a 'disponible' requiere justificación y 'fallecida' es irreversible."
            });
        }

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

        return Ok(new MascotaRespuestaDto
        {
            IdMascota = mascotaExistente.IdMascota,
            Nombre = mascotaExistente.Nombre,
            Especie = mascotaExistente.Especie,
            Tamano = mascotaExistente.Tamano,
            EdadMeses = mascotaExistente.EdadMeses,
            EstadoSalud = mascotaExistente.EstadoSalud,
            Estado = mascotaExistente.Estado,
            FechaRegistro = mascotaExistente.FechaRegistro,
            ImagenUrl = mascotaExistente.ImagenUrl
        });
    }

    // ============================================================
    // 5) MÉTODOS [HttpDelete]
    // ============================================================

    /// <summary>
    /// [HU-09] Elimina físicamente una mascota del sistema.
    /// </summary>
    /// <param name="id">Identificador único de la mascota a eliminar.</param>
    /// <returns>ActionResult con un mensaje de confirmación en formato JSON.</returns>
    /// <response code="200">Mascota eliminada correctamente.</response>
    /// <response code="404">Mascota no encontrada.</response>
    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarMascota(long id)
    {
        var mascota = await _context.Mascota.FindAsync(id);
        if (mascota is null)
            return NotFound(new { error = "Mascota no encontrada." });

        _context.Mascota.Remove(mascota);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Mascota eliminada del sistema." });
    }

    // ============================================================
    // REGLA DE NEGOCIO PRIVADA COMPARTIDA
    // ============================================================

    /// <summary>
    /// Regla de negocio de transición de estado:
    /// pasar de "adoptada" a "disponible" exige justificación y "fallecida" es irreversible.
    /// </summary>
    /// <param name="estadoActual">Estado actual de la mascota.</param>
    /// <param name="nuevoEstado">Estado propuesto para la mascota.</param>
    /// <param name="justificacion">Justificación del cambio de estado (opcional).</param>
    /// <returns>true si la transición es válida; false en caso contrario.</returns>
    private static bool EsTransicionEstadoValida(string estadoActual, string nuevoEstado, string? justificacion)
    {
        if (estadoActual == "adoptada" && nuevoEstado == "disponible" && string.IsNullOrWhiteSpace(justificacion))
            return false;

        if (estadoActual == "fallecida" && nuevoEstado != "fallecida")
            return false;

        return true;
    }
}

/// <summary>
/// DTO de respuesta con los datos públicos de una mascota.
/// Evita el ciclo infinito de JSON al incluir el refugio y oculta el BLOB de la imagen.
/// </summary>
public sealed class MascotaRespuestaDto
{
    /// <summary>Identificador único de la mascota.</summary>
    public long IdMascota { get; set; }

    /// <summary>Nombre de la mascota.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Especie de la mascota: perro, gato u otro.</summary>
    public string Especie { get; set; } = string.Empty;

    /// <summary>Tamaño de la mascota: pequeño, mediano o grande.</summary>
    public string Tamano { get; set; } = string.Empty;

    /// <summary>Edad de la mascota expresada en meses.</summary>
    public int EdadMeses { get; set; }

    /// <summary>Estado de salud: sano, en_tratamiento, discapacidad o crónico.</summary>
    public string EstadoSalud { get; set; } = string.Empty;

    /// <summary>Estado de adopción: disponible, adoptada, fallecida, en_tratamiento o reservada.</summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>Fecha de registro de la mascota (UTC).</summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>URL externa de la imagen de la mascota, si existe.</summary>
    public string? ImagenUrl { get; set; }

    /// <summary>Datos del refugio que resguarda a la mascota (presente solo en el filtro por ubicación).</summary>
    public RefugioRespuestaDto? Refugio { get; set; }
}

/// <summary>
/// DTO de respuesta con los datos públicos de ubicación de un refugio.
/// </summary>
public sealed class RefugioRespuestaDto
{
    /// <summary>Nombre de la organización del refugio.</summary>
    public string NombreOrganizacion { get; set; } = string.Empty;

    /// <summary>Departamento donde se ubica el refugio.</summary>
    public string Departamento { get; set; } = string.Empty;

    /// <summary>Municipio donde se ubica el refugio.</summary>
    public string Municipio { get; set; } = string.Empty;
}