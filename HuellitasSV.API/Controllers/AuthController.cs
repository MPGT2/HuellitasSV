// [SEGURIDAD] Michael Menendez: Autenticación central con JWT.
// Login único para todas las cuentas (Admin, Refugio, Usuario): valida credenciales
// y estado, emite un token JWT firmado con el rol y el identificador del perfil
// (idRefugio o idUsuario) para que los endpoints protegidos no confíen en parámetros
// enviados por el cliente. Los logins de HU-01 y HU-02 también emiten su token
// específico, manteniendo su respuesta original para no romper el front.

namespace HuellitasSV.API.Controllers;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using HuellitasSV.API.Security;

/// <summary>
/// Controlador de autenticación con JWT: emite tokens firmados para cuentas válidas
/// de cualquier rol (Admin, Refugio o Usuario).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtTokenService _tokenService;

    /// <summary>Componente de hash de contraseñas (PBKDF2, compartido con HU-01/HU-02).</summary>
    private static readonly PasswordHasher<Cuenta> _hasher = new();

    /// <summary>
    /// Inicializa el controlador con el contexto de base de datos y el servicio de tokens.
    /// </summary>
    public AuthController(ApplicationDbContext context, JwtTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Autentica cualquier cuenta (Admin, Refugio o Usuario) y devuelve un JWT con el rol
    /// y el identificador del perfil vinculado. Reglas: cuentas con estado "inactivo" o
    /// "bloqueado" no acceden; refugios y admins requieren estado "aprobado".
    /// </summary>
    /// <param name="dto">Credenciales (JSON): correo y contraseña.</param>
    /// <returns>Token JWT y datos de la cuenta autenticada.</returns>
    /// <response code="200">Autenticación exitosa; devuelve el token.</response>
    /// <response code="401">Credenciales incorrectas (error genérico).</response>
    /// <response code="403">Cuenta inactiva, bloqueada o pendiente de aprobación.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Login(LoginUsuarioDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();

        // Error genérico sin revelar si falló el correo o la contraseña.
        var cuenta = await _context.Cuenta.FirstOrDefaultAsync(c => c.Correo == correo);
        if (cuenta == null || !VerificarContrasena(cuenta, dto.Contrasena))
        {
            return Unauthorized(new { error = "Credenciales incorrectas." });
        }

        // Regla de negocio: estados que impiden el acceso según el rol.
        if (cuenta.Rol == "Usuario")
        {
            if (cuenta.Estado == "inactivo" || cuenta.Estado == "bloqueado")
            {
                var motivo = cuenta.Estado == "inactivo" ? "inactiva" : "bloqueada";
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = $"Acceso denegado: su cuenta está {motivo}. Contacte al administrador."
                });
            }
        }
        else if (cuenta.Estado != "aprobado" && cuenta.Estado != "activo")
        {
            var motivo = cuenta.Estado == "pendiente"
                ? "pendiente de aprobación"
                : "rechazada";
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = $"Acceso denegado: la cuenta está {motivo}."
            });
        }

        // Identificadores del perfil vinculado a la cuenta.
        long? idRefugio = null;
        long? idUsuario = null;
        string? nombre = null;

        if (cuenta.Rol == "Refugio")
        {
            var refugio = await _context.Refugio.FirstOrDefaultAsync(r => r.IdCuenta == cuenta.IdCuenta);
            idRefugio = refugio?.IdRefugio;
            nombre = refugio?.NombreOrganizacion;
        }
        else if (cuenta.Rol == "Usuario")
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdCuenta == cuenta.IdCuenta);
            idUsuario = usuario?.IdUsuario;
            nombre = usuario?.Nombre;
        }

        var token = _tokenService.GenerarToken(cuenta.IdCuenta, cuenta.Rol, nombre, idRefugio, idUsuario);

        return Ok(new
        {
            mensaje = "Autenticación exitosa.",
            token,
            idCuenta = cuenta.IdCuenta,
            rol = cuenta.Rol,
            nombre,
            idRefugio,
            idUsuario
        });
    }

    /// <summary>
    /// Verifica la contraseña contra el hash PBKDF2 almacenado.
    /// </summary>
    private bool VerificarContrasena(Cuenta cuenta, string contrasena)
    {
        var resultado = _hasher.VerifyHashedPassword(cuenta, cuenta.Contrasena, contrasena);
        return resultado != PasswordVerificationResult.Failed;
    }
}