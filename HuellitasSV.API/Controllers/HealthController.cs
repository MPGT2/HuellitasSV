using System.Data.Common;
using HuellitasSV.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Sonda de diagnostico del despliegue: confirma que la API responde y que logra
/// abrir la conexion a la base de datos. Sin este endpoint un fallo de conexion
/// se manifestaba como un 500 opaco en todos los controladores.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>Inicializa una nueva instancia de <see cref="HealthController"/>.</summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifica el estado de la API y de la conexion a la base de datos.
    /// </summary>
    /// <remarks>
    /// Devuelve 200 con <c>bd: "conectada"</c> cuando todo esta bien, o 503 con el
    /// detalle del error cuando la base de datos no responde. El mensaje se
    /// sanitiza para no exponer la cadena de conexion.
    /// </remarks>
    /// <returns>Estado de la API y de la base de datos.</returns>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var hayCadena = !string.IsNullOrWhiteSpace(
            _context.Database.GetConnectionString());

        if (!hayCadena)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                estado = "error",
                bd = "sin cadena de conexion",
                detalle = "ConnectionStrings:DefaultConnection no esta configurada (appsettings.json o variable de entorno ConnectionStrings__DefaultConnection)."
            });
        }

        try
        {
            // CanConnectAsync se traga la excepcion y solo devuelve false, dejando
            // el 500 sin causa. OpenConnectionAsync si lanza el error real de red
            // (timeout, host inaccesible, credenciales), que es lo que hay que ver.
            var conexion = _context.Database.GetDbConnection();
            await conexion.OpenAsync(cancellationToken);

            var detalle = $"conectada a {conexion.DataSource} / {conexion.Database}";
            await conexion.CloseAsync();

            return StatusCode(StatusCodes.Status200OK, new
            {
                estado = "ok",
                bd = "conectada",
                servidor = conexion.DataSource,
                baseDatos = conexion.Database,
                mensaje = detalle,
                utc = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            var sql = ex as Microsoft.Data.SqlClient.SqlException;

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                estado = "error",
                bd = "conexion fallida",
                tipo = ex.GetType().Name,
                numero = sql?.Number,
                detalle = Sanitizar(ex.Message),
                utc = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Quita de un mensaje de error cualquier fragmento que huela a credenciales
    /// (cadena de conexion o contrasena) antes de devolverlo al cliente.
    /// </summary>
    private static string Sanitizar(string mensaje)
    {
        var limpio = System.Text.RegularExpressions.Regex.Replace(
            mensaje, @"(?i)(password|pwd)\s*=\s*[^;]*", "password=***");
        return System.Text.RegularExpressions.Regex.Replace(
            limpio, @"(?i)(user\s*id|uid)\s*=\s*[^;]*", "user id=***");
    }
}