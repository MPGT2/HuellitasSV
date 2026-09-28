// [HU-13] Oscar Ramirez: Gestión y cobro de espacios publicitarios a tiendas (administrador).
// Endpoints: GET listar, GET refugio/{id}, GET panel/monetizacion,
// POST crear solicitud, POST {id}/aprobar, POST {id}/confirmar-pago, POST {id}/rechazar.
// La expiración a "vencido" se recalcula automáticamente en cada consulta, ya que el
// proyecto no cuenta con un job en segundo plano (scheduler).

namespace HuellitasSV.API.Controllers;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasSV.API.Data;
using HuellitasSV.API.DTOs;
using HuellitasSV.API.Models;

/// <summary>
/// Controlador para que el administrador gestione y cobre espacios publicitarios a las tiendas.
/// [SEGURIDAD] Crear solicitud es público (lo hace la tienda interesada); aprobar, confirmar pago,
/// rechazar y ver el panel de monetización exigen token JWT de rol "Admin".
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnunciosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Radio de cobertura en kilómetros para considerar una tienda "cercana" a un refugio.
    /// </summary>
    private const double RadioCoberturaKm = 5.0;

    /// <summary>
    /// Inicializa el controlador con el contexto de base de datos.
    /// </summary>
    public AnunciosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // -------------------------------------------------------------------------
    // 1) MÉTODOS [HttpGet]
    // -------------------------------------------------------------------------

    /// <summary>
    /// Lista los anuncios. Por defecto solo devuelve los visibles para la comunidad:
    /// estado "activo", con pago confirmado y dentro de su rango de fechas.
    /// Use estado=pendiente, estado=vencido o estado=todas para la vista de administración.
    /// </summary>
    /// <response code="200">Lista de anuncios.</response>
    [HttpGet]
    public async Task<IActionResult> GetAnuncios([FromQuery] string? estado = "activo")
    {
        // Criterio de aceptación 3: expira automáticamente lo que ya venció antes de listar.
        await ActualizarVencidosAsync();

        var query = _context.Anuncios.AsQueryable();
        var ahora = DateTime.UtcNow;

        if (string.IsNullOrEmpty(estado) || estado.Equals("activo", StringComparison.OrdinalIgnoreCase))
        {
            // Vista pública: lo realmente visible a los usuarios (criterio de aceptación 2).
            query = query.Where(a => a.Estado == "activo"
                && a.PagoConfirmado
                && a.FechaInicio <= ahora
                && a.FechaFin >= ahora);
        }
        else if (!estado.Equals("todas", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(a => a.Estado == estado.ToLower());
        }

        var anuncios = await query
            .OrderByDescending(a => a.FechaCreacion)
            .Select(a => new
            {
                a.IdAnuncio,
                a.NombreTienda,
                a.ContactoTienda,
                a.ImagenUrl,
                a.Descripcion,
                a.Precio,
                a.FechaInicio,
                a.FechaFin,
                a.PagoConfirmado,
                a.Estado,
                a.FechaAprobacion
            })
            .ToListAsync();

        return Ok(anuncios);
    }

    /// <summary>
    /// Obtiene los anuncios de tiendas cercanas a un refugio, para mostrarlos como
    /// sección de publicidad en su catálogo. Si no hay anuncios activos en la zona
    /// (o el refugio no tiene ubicación), se devuelve una lista vacía.
    /// </summary>
    /// <param name="idRefugio">Identificador del refugio cuyo catálogo se está navegando.</param>
    /// <response code="200">Lista de anuncios cercanos (puede ser vacía).</response>
    /// <response code="404">El refugio indicado no existe.</response>
    [HttpGet("refugio/{idRefugio}")]
    public async Task<IActionResult> GetAnunciosPorRefugio(long idRefugio)
    {
        await ActualizarVencidosAsync();

        var refugio = await _context.Refugio.FindAsync(idRefugio);
        if (refugio is null)
            return NotFound(new { error = "El refugio indicado no existe." });

        if (refugio.Latitud is null || refugio.Longitud is null)
            return Ok(Array.Empty<object>());

        var ahora = DateTime.UtcNow;

        var anunciosVisibles = await _context.Anuncios
            .Where(a => a.Estado == "activo"
                && a.PagoConfirmado
                && a.FechaInicio <= ahora
                && a.FechaFin >= ahora
                && a.Latitud != null
                && a.Longitud != null)
            .ToListAsync();

        var anunciosCercanos = anunciosVisibles
            .Where(a => DistanciaKm(
                refugio.Latitud!.Value, refugio.Longitud!.Value,
                a.Latitud!.Value, a.Longitud!.Value) <= RadioCoberturaKm)
            .OrderByDescending(a => a.FechaAprobacion)
            .Select(a => new
            {
                a.IdAnuncio,
                a.NombreTienda,
                a.ContactoTienda,
                a.ImagenUrl,
                a.Descripcion,
                a.Latitud,
                a.Longitud
            })
            .ToList();

        return Ok(anunciosCercanos);
    }

    /// <summary>
    /// Panel de monetización para el Administrador Global: ingresos del mes actual
    /// y cantidad de campañas por estado (activas, vencidas, pendientes).
    /// Sin anuncios registrados, todos los indicadores se muestran en 0.
    /// </summary>
    /// <response code="200">Indicadores de monetización del mes actual.</response>
    [HttpGet("panel/monetizacion")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMonetizacion()
    {
        await ActualizarVencidosAsync();

        var ahora = DateTime.UtcNow;

        var ingresosMes = await _context.Anuncios
            .Where(a => a.PagoConfirmado
                && a.FechaAprobacion != null
                && a.FechaAprobacion.Value.Year == ahora.Year
                && a.FechaAprobacion.Value.Month == ahora.Month)
            .SumAsync(a => (decimal?)a.Precio) ?? 0;

        var campanasActivas = await _context.Anuncios.CountAsync(a => a.Estado == "activo");
        var campanasVencidas = await _context.Anuncios.CountAsync(a => a.Estado == "vencido");
        var campanasPendientes = await _context.Anuncios.CountAsync(a => a.Estado == "pendiente");

        return Ok(new
        {
            anio = ahora.Year,
            mes = ahora.Month,
            ingresosMes,
            campanasActivas,
            campanasVencidas,
            campanasPendientes
        });
    }

    // -------------------------------------------------------------------------
    // 2) MÉTODOS [HttpPost]
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registra la solicitud de una tienda para publicar un anuncio. Queda en
    /// estado "pendiente" hasta que el administrador la revise.
    /// </summary>
    /// <response code="201">Solicitud registrada.</response>
    /// <response code="400">Datos inválidos (ej. fecha de fin anterior a la de inicio).</response>
    [HttpPost]
    public async Task<IActionResult> CrearAnuncio([FromBody] CrearAnuncioDto dto)
    {
        if (dto.FechaFin <= dto.FechaInicio)
            return BadRequest(new { error = "La fecha de fin debe ser posterior a la fecha de inicio." });

        var anuncio = new Anuncio
        {
            NombreTienda = dto.NombreTienda,
            ContactoTienda = dto.ContactoTienda,
            Latitud = dto.Latitud,
            Longitud = dto.Longitud,
            ImagenUrl = dto.ImagenUrl,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            PagoConfirmado = false,
            Estado = "pendiente",
            FechaCreacion = DateTime.UtcNow
        };

        _context.Anuncios.Add(anuncio);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAnuncios), new { id = anuncio.IdAnuncio }, anuncio);
    }

    /// <summary>
    /// Aprueba un anuncio pendiente. Queda visible a los usuarios solo cuando además
    /// el pago esté confirmado y su fecha de inicio haya llegado.
    /// </summary>
    /// <response code="200">Anuncio aprobado.</response>
    /// <response code="400">El anuncio no está en estado "pendiente".</response>
    /// <response code="404">Anuncio no encontrado.</response>
    [HttpPost("{id}/aprobar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AprobarAnuncio(long id)
    {
        var anuncio = await _context.Anuncios.FindAsync(id);
        if (anuncio == null)
            return NotFound(new { error = "Anuncio no encontrado." });

        if (anuncio.Estado != "pendiente")
            return BadRequest(new { error = $"Solo se pueden aprobar anuncios en estado 'pendiente'. Estado actual: {anuncio.Estado}." });

        anuncio.Estado = "activo";
        anuncio.FechaAprobacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Anuncio aprobado y activado.",
            anuncio.IdAnuncio,
            anuncio.Estado,
            anuncio.PagoConfirmado,
            aviso = anuncio.PagoConfirmado
                ? "El anuncio ya es visible para los usuarios (según su rango de fechas)."
                : "El anuncio NO será visible para los usuarios hasta que se confirme el pago."
        });
    }

    /// <summary>
    /// Confirma el pago de un anuncio. Si el pago no está confirmado al llegar la
    /// fecha de activación, el anuncio no se publica hasta registrar el pago aquí.
    /// </summary>
    /// <response code="200">Pago confirmado.</response>
    /// <response code="400">El anuncio ya está vencido.</response>
    /// <response code="404">Anuncio no encontrado.</response>
    [HttpPost("{id}/confirmar-pago")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConfirmarPago(long id)
    {
        var anuncio = await _context.Anuncios.FindAsync(id);
        if (anuncio == null)
            return NotFound(new { error = "Anuncio no encontrado." });

        if (anuncio.Estado == "vencido")
            return BadRequest(new { error = "No se puede confirmar el pago de un anuncio vencido." });

        anuncio.PagoConfirmado = true;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Pago confirmado.",
            anuncio.IdAnuncio,
            anuncio.PagoConfirmado,
            anuncio.Estado
        });
    }

    /// <summary>
    /// Rechaza un anuncio pendiente. Completa el flujo de administración (aprobar/rechazar).
    /// </summary>
    /// <response code="200">Anuncio rechazado.</response>
    /// <response code="400">El anuncio no está en estado "pendiente".</response>
    /// <response code="404">Anuncio no encontrado.</response>
    [HttpPost("{id}/rechazar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RechazarAnuncio(long id)
    {
        var anuncio = await _context.Anuncios.FindAsync(id);
        if (anuncio == null)
            return NotFound(new { error = "Anuncio no encontrado." });

        if (anuncio.Estado != "pendiente")
            return BadRequest(new { error = "Solo se pueden rechazar anuncios en estado 'pendiente'." });

        anuncio.Estado = "rechazado";
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Anuncio rechazado.", anuncio.IdAnuncio, anuncio.Estado });
    }

    // -------------------------------------------------------------------------
    // Helpers privados
    // -------------------------------------------------------------------------

    /// <summary>
    /// Cuando un anuncio vence (llega su fecha de fin), lo pasa a estado "vencido".
    /// </summary>
    private async Task ActualizarVencidosAsync()
    {
        var ahora = DateTime.UtcNow;
        var vencidos = await _context.Anuncios
            .Where(a => a.Estado == "activo" && a.FechaFin < ahora)
            .ToListAsync();

        if (vencidos.Count == 0) return;

        foreach (var anuncio in vencidos)
            anuncio.Estado = "vencido";

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calcula la distancia en kilómetros entre dos puntos geográficos (Haversine).
    /// </summary>
    private static double DistanciaKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double radioTierraKm = 6371;

        var deltaLat = GradosARadianes(lat2 - lat1);
        var deltaLon = GradosARadianes(lon2 - lon1);

        var a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(GradosARadianes(lat1)) * Math.Cos(GradosARadianes(lat2)) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        return 2 * radioTierraKm * Math.Asin(Math.Sqrt(a));
    }

    /// <summary>Convierte grados a radianes.</summary>
    private static double GradosARadianes(double grados) => grados * Math.PI / 180.0;
}
