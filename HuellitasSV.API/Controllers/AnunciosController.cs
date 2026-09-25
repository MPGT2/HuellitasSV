// [HU-XX] <Tu nombre>: Gestión y cobro de espacios publicitarios a tiendas (administrador).
// Endpoints: GET listar, POST crear solicitud, POST {id}/aprobar, POST {id}/confirmar-pago, POST {id}/rechazar.
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
/// [SEGURIDAD] Crear solicitud es público (lo hace la tienda interesada); aprobar, confirmar pago
/// y rechazar exigen token JWT de rol "Admin".
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnunciosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa el controlador con el contexto de base de datos.
    /// </summary>
    public AnunciosController(ApplicationDbContext context)
    {
        _context = context;
    }

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
    /// Aprueba un anuncio pendiente. Criterio de aceptación: al aprobarlo, el sistema
    /// lo activa; queda realmente visible a los usuarios solo cuando además el pago
    /// esté confirmado y su fecha de inicio haya llegado.
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
    /// Confirma el pago de un anuncio. Criterio de aceptación: si el pago no está
    /// confirmado al llegar la fecha de activación, el anuncio no se publica hasta
    /// que se registre el pago mediante este endpoint.
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
    /// Rechaza un anuncio pendiente. No forma parte de los criterios de aceptación
    /// originales, pero completa el flujo de administración (aprobar/rechazar).
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

    /// <summary>
    /// Criterio de aceptación: cuando un anuncio vence (llega su fecha de fin),
    /// el sistema lo pasa automáticamente a estado "vencido" y deja de mostrarlo.
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
}