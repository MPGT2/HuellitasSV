// [HU-01] Michael Menendez: Controlador de acceso para usuarios clientes.
// Registro: crea la cuenta con rol "Usuario" y estado "activo" y confirma el registro.
// Inicio de sesión: autentica por correo y contraseña; ante credenciales incorrectas
// responde con un error genérico y ante cuentas inactivas o bloqueadas deniega el
// acceso informando el motivo.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;

namespace HuellitasSV.API.Controllers
{
    /// <summary>
    /// Controlador que gestiona el acceso de los usuarios clientes a la plataforma:
    /// registro e inicio de sesión (HU-01).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>Componente de hash de contraseñas (PBKDF2, sin estado, seguro en hilos).</summary>
        private static readonly PasswordHasher<Cuenta> _hasher = new();

        /// <summary>
        /// Inicializa el controlador con el contexto de base de datos inyectado.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework Core de HuellitasSV.</param>
        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1) MÉTODOS [HttpPost]
        // ============================================================

        /// <summary>
        /// [HU-01] Registra un nuevo usuario cliente: valida los campos obligatorios,
        /// que el correo no exista en el sistema y crea la cuenta con rol "Usuario"
        /// y estado "activo", confirmando el registro de inmediato.
        /// </summary>
        /// <param name="dto">Datos del usuario a registrar (JSON): nombre, correo y contraseña.</param>
        /// <returns>Confirmación del registro con el identificador generado.</returns>
        /// <response code="201">Usuario registrado correctamente.</response>
        /// <response code="400">Si algún campo obligatorio está vacío o mal formado.</response>
        /// <response code="409">Si el correo ya está registrado.</response>
        [HttpPost("registro")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> RegistrarUsuario(RegistroUsuarioDto dto)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();
            var nombre = dto.Nombre.Trim();

            // Regla de negocio HU-01: el correo no debe existir en el sistema.
            var existeCorreo = await _context.Cuenta.AnyAsync(c => c.Correo == correo);
            if (existeCorreo)
            {
                return Conflict(new { error = "El correo ya está registrado." });
            }

            // HU-01: la cuenta se crea con rol "Usuario" y estado "activo" (contraseña con hash PBKDF2).
            var cuenta = new Cuenta
            {
                Correo = correo,
                Contrasena = _hasher.HashPassword(null!, dto.Contrasena),
                Rol = "Usuario",
                Estado = "activo"
            };
            _context.Cuenta.Add(cuenta);
            await _context.SaveChangesAsync();

            // Perfil del usuario vinculado a su cuenta.
            var usuario = new Usuario
            {
                IdCuenta = cuenta.IdCuenta,
                Nombre = nombre
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new
            {
                mensaje = "Usuario registrado correctamente. Ya puede iniciar sesión.",
                idUsuario = usuario.IdUsuario,
                idCuenta = cuenta.IdCuenta,
                nombre = usuario.Nombre,
                correo = cuenta.Correo,
                rol = cuenta.Rol,
                estado = cuenta.Estado
            });
        }

        /// <summary>
        /// [HU-01] Inicia sesión para un usuario cliente validando sus credenciales.
        /// Si las credenciales son incorrectas responde con un error genérico (sin
        /// indicar qué campo falló); si la cuenta está inactiva o bloqueada, deniega
        /// el acceso informando el motivo.
        /// </summary>
        /// <param name="dto">Credenciales del usuario (JSON): correo y contraseña.</param>
        /// <returns>Confirmación de autenticación con los datos del usuario.</returns>
        /// <response code="200">Autenticación exitosa.</response>
        /// <response code="401">Si las credenciales son incorrectas (error genérico).</response>
        /// <response code="403">Si la cuenta está inactiva o bloqueada (se informa el motivo).</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> Login(LoginUsuarioDto dto)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            // Regla de negocio HU-01: error genérico sin revelar si falló el correo o la contraseña.
            var cuenta = await _context.Cuenta.FirstOrDefaultAsync(c => c.Correo == correo);
            if (cuenta == null || cuenta.Rol != "Usuario" || !await VerificarContrasenaAsync(cuenta, dto.Contrasena))
            {
                return Unauthorized(new { error = "Credenciales incorrectas." });
            }

            // Regla de negocio HU-01: cuentas inactivas o bloqueadas no pueden entrar; se informa el motivo.
            if (cuenta.Estado == "inactivo" || cuenta.Estado == "bloqueado")
            {
                var motivo = cuenta.Estado == "inactivo" ? "inactiva" : "bloqueada";
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = $"Acceso denegado: su cuenta está {motivo}. Contacte al administrador."
                });
            }

            if (cuenta.Estado != "activo")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = $"Acceso denegado. El estado actual de su cuenta es: {cuenta.Estado}."
                });
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdCuenta == cuenta.IdCuenta);
            if (usuario == null)
            {
                return NotFound(new { error = "La cuenta no tiene un perfil de usuario asociado." });
            }

            return Ok(new
            {
                mensaje = "Autenticación exitosa.",
                idUsuario = usuario.IdUsuario,
                nombre = usuario.Nombre,
                correo = cuenta.Correo
            });
        }

        // ============================================================
        // 2) REGLAS DE NEGOCIO PRIVADAS COMPARTIDAS
        // ============================================================

        /// <summary>
        /// Verifica una contraseña contra el hash almacenado (PBKDF2). Da soporte de
        /// migración progresiva: las cuentas creadas antes del hashing (contraseña en
        /// texto plano) se convierten automáticamente a hash al iniciar sesión.
        /// </summary>
        /// <param name="cuenta">Cuenta cuya contraseña se verifica.</param>
        /// <param name="contrasena">Contraseña proporcionada por el cliente.</param>
        /// <returns>true si la contraseña es correcta; false en caso contrario.</returns>
        private async Task<bool> VerificarContrasenaAsync(Cuenta cuenta, string contrasena)
        {
            PasswordVerificationResult resultado;
            try
            {
                resultado = _hasher.VerifyHashedPassword(cuenta, cuenta.Contrasena, contrasena);
            }
            catch (FormatException)
            {
                // El valor almacenado no es un hash (cuenta legada en texto plano).
                resultado = PasswordVerificationResult.Failed;
            }

            if (resultado == PasswordVerificationResult.Success)
                return true;

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                cuenta.Contrasena = _hasher.HashPassword(cuenta, contrasena);
                await _context.SaveChangesAsync();
                return true;
            }

            // Cuenta legada: comparación directa con actualización automática a hash.
            if (cuenta.Contrasena == contrasena)
            {
                cuenta.Contrasena = _hasher.HashPassword(cuenta, contrasena);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }

    // ============================================================
    // DTOs (declarados al final del archivo)
    // ============================================================

    /// <summary>
    /// Datos requeridos para registrar un nuevo usuario cliente (HU-01).
    /// </summary>
    public class RegistroUsuarioDto
    {
        /// <summary>Nombre completo del usuario.</summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Correo de la cuenta (debe ser único en el sistema).</summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [MaxLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        /// <summary>Contraseña de la cuenta (mínimo 8 caracteres).</summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(255, ErrorMessage = "La contraseña no puede exceder 255 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;
    }

    /// <summary>
    /// Credenciales para el inicio de sesión de un usuario cliente (HU-01).
    /// </summary>
    public class LoginUsuarioDto
    {
        /// <summary>Correo de la cuenta.</summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Correo { get; set; } = string.Empty;

        /// <summary>Contraseña de la cuenta.</summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contrasena { get; set; } = string.Empty;
    }
}