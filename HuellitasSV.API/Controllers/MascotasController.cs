// [HU-04 | HU-05] Michael Menendez: Controlador de mascotas (versión 2) - Etapas 1 y 2 de la consolidación en un único archivo.
// HU-04 reemplaza el antiguo partial: MascotasController.CatalogoEspecie.cs; HU-05 reemplaza: MascotasController.FiltroAtributos.cs.
// Los endpoints de HU-06 y HU-09 se incorporarán en sus respectivas ramas feature.
// Orden jerárquico: 1) Constructor, 2) [HttpGet], 3) [HttpPost], 4) [HttpPut], 5) [HttpDelete].

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
/// Controlador (parcial) de mascotas de HuellitasSV. En esta etapa incluye el filtro por especie (HU-04)
/// y el filtro por atributos (HU-05); se mantiene parcial hasta consolidar HU-06 y HU-09.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public partial class MascotasController : ControllerBase
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
}