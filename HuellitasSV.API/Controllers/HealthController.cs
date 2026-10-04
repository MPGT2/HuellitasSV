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
    /// Prueba de red a bajo nivel: separa el fallo de DNS del fallo de conexion TCP.
    /// </summary>
    /// <remarks>
    /// El error 258 del TCP Provider no distingue "no resuelve el nombre" de "el
    /// puerto esta filtrado". Este endpoint hace las dos pruebas por separado e
    /// informa el tiempo que tardo cada una.
    /// </remarks>
    /// <returns>Resultado de la resolucion DNS y del handshake TCP con el puerto 1433.</returns>
    [HttpGet("red")]
    public async Task<IActionResult> Red(CancellationToken cancellationToken)
    {
        var conexion = (Microsoft.Data.SqlClient.SqlConnection)_context.Database.GetDbConnection();
        var servidor = conexion.DataSource;

        // SqlConnection no expone el puerto; la cadena no lo define, asi que se
        // usa el estandar de SQL Server. La prueba es solo de diagnostico.
        const int puerto = 1433;

        string? dns = null;
        string? tcp = null;
        bool dnsOk = false;
        bool tcpOk = false;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var direcciones = await System.Net.Dns.GetHostAddressesAsync(servidor, cancellationToken);
            dns = string.Join(", ", direcciones.Select(a => a.ToString()));
            dnsOk = true;
        }
        catch (Exception ex)
        {
            dns = $"ERROR {ex.GetType().Name}: {ex.Message}";
        }

        var swDns = sw.ElapsedMilliseconds;

        var cronometroTcp = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var cliente = new System.Net.Sockets.TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            await cliente.ConnectAsync(servidor, puerto, cts.Token);
            tcp = $"conectado a {servidor}:{puerto}";
            tcpOk = true;
        }
        catch (Exception ex)
        {
            tcp = $"ERROR {ex.GetType().Name}: {ex.Message}";
        }

        return StatusCode(
            dnsOk && tcpOk ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable,
            new
            {
                estado = dnsOk && tcpOk ? "ok" : "error",
                servidor,
                puerto,
                dnsResuelve = dnsOk,
                dns,
                dnsMilisegundos = swDns,
                tcpConecta = tcpOk,
                tcp,
                tcpMilisegundos = cronometroTcp.ElapsedMilliseconds,
                conclusion = dnsOk
                    ? (tcpOk
                        ? "DNS y TCP funcionan: el fallo esta en la negociacion TDS/TLS o en las credenciales."
                        : "El DNS resuelve pero el puerto TCP no acepta conexiones: filtrado por firewall o puerto cerrado.")
                    : "El nombre del servidor no resuelve desde el contenedor: problema de DNS.",
                utc = DateTime.UtcNow
            });
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