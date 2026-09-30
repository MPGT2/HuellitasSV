using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using HuellitasSV.API.Services;

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

        /// <summary>Servicio de emisión de tokens JWT (seguridad).</summary>
        private readonly Security.JwtTokenService _tokenService;

        private readonly IPasswordService _passwords;
        private readonly IAccountStatePolicy _estadoCuenta;
        private readonly ICurrentUserService _usuarioActual;

        /// <summary>
        /// Inicializa el controlador con el contexto de base de datos y los servicios compartidos.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework Core de HuellitasSV.</param>
        /// <param name="tokenService">Servicio de emisión de tokens JWT.</param>
        /// <param name="passwords">Servicio compartido de cifrado y verificación de contraseñas.</param>
        /// <param name="estadoCuenta">Regla compartida de estado de cuenta.</param>
        /// <param name="usuarioActual">Claims del token ya convertidos.</param>
        public UsuariosController(
            ApplicationDbContext context,
            Security.JwtTokenService tokenService,
            IPasswordService passwords,
            IAccountStatePolicy estadoCuenta,
            ICurrentUserService usuarioActual)
        {
            _context = context;
            _tokenService = tokenService;
            _passwords = passwords;
            _estadoCuenta = estadoCuenta;
            _usuarioActual = usuarioActual;
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
                Contrasena = _passwords.Hash(dto.Contrasena),
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
            if (cuenta == null || cuenta.Rol != "Usuario" || !await _passwords.VerificarAsync(cuenta, dto.Contrasena))
            {
                return Unauthorized(new { error = "Credenciales incorrectas." });
            }

            // Regla de negocio HU-01: cuentas inactivas o bloqueadas no pueden entrar; se informa el motivo.
            if (_estadoCuenta.EstaBloqueada(cuenta))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = _estadoCuenta.ConstruirMensajeBloqueo(cuenta)
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
                token = _tokenService.GenerarToken(cuenta.IdCuenta, cuenta.Rol, usuario.Nombre, null, usuario.IdUsuario),
                idUsuario = usuario.IdUsuario,
                idCuenta = cuenta.IdCuenta,
                nombre = usuario.Nombre,
                rol = cuenta.Rol,
                correo = cuenta.Correo
            });
        }

        // ============================================================
        // 3) MÉTODOS [HttpGet] - Obtener perfil y actualizar
        // ============================================================

        /// <summary>
        /// Obtiene el perfil del usuario autenticado.
        /// </summary>
        /// <returns>Perfil del usuario.</returns>
        /// <response code="200">Perfil del usuario.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpGet("perfil")]
        [Authorize(Roles = "Usuario")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetPerfil()
        {
            var idUsuarioToken = _usuarioActual.ObtenerUsuarioId();

            if (idUsuarioToken <= 0)
                return Unauthorized(new { error = "El token no incluye el perfil de usuario asociado." });

            var usuario = await _context.Usuarios
                .Include(u => u.Cuenta)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuarioToken);

            if (usuario == null)
                return NotFound(new { error = "Usuario no encontrado." });

            return Ok(new
            {
                usuario.IdUsuario,
                usuario.Nombre,
                Correo = usuario.Cuenta?.Correo,
                Rol = usuario.Cuenta?.Rol,
                Estado = usuario.Cuenta?.Estado
            });
        }

        /// <summary>
        /// Actualiza el perfil del usuario autenticado.
        /// </summary>
        /// <param name="dto">Datos a actualizar (nombre, correo, contraseña).</param>
        /// <returns>Perfil actualizado.</returns>
        /// <response code="200">Perfil actualizado correctamente.</response>
        /// <response code="400">Datos inválidos o correo ya existente.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpPut("perfil")]
        [Authorize(Roles = "Usuario")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ActualizarPerfil(ActualizarPerfilUsuarioDto dto)
        {
            var idUsuarioToken = _usuarioActual.ObtenerUsuarioId();

            if (idUsuarioToken <= 0)
                return Unauthorized(new { error = "El token no incluye el perfil de usuario asociado." });

            var usuario = await _context.Usuarios
                .Include(u => u.Cuenta)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuarioToken);

            if (usuario == null)
                return NotFound(new { error = "Usuario no encontrado." });

            if (!string.IsNullOrEmpty(dto.Nombre))
                usuario.Nombre = dto.Nombre.Trim();

            var cuenta = usuario.Cuenta;
            if (cuenta == null)
                return NotFound(new { error = "La cuenta asociada no existe." });

            if (!string.IsNullOrEmpty(dto.Correo))
            {
                var nuevoCorreo = dto.Correo.Trim().ToLowerInvariant();
                var existeCorreo = await _context.Cuenta.AnyAsync(c => c.Correo == nuevoCorreo && c.IdCuenta != cuenta.IdCuenta);
                if (existeCorreo)
                    return BadRequest(new { error = "El correo ya está registrado por otra cuenta." });

                cuenta.Correo = nuevoCorreo;
            }

            if (!string.IsNullOrEmpty(dto.Contrasena))
            {
                if (dto.Contrasena.Length < 8)
                    return BadRequest(new { error = "La contraseña debe tener al menos 8 caracteres." });

                cuenta.Contrasena = _passwords.Hash(cuenta, dto.Contrasena);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Perfil actualizado correctamente.",
                usuario.IdUsuario,
                usuario.Nombre,
                cuenta.Correo,
                cuenta.Rol,
                cuenta.Estado
            });
        }

        /// <summary>
        /// Elimina la cuenta del usuario autenticado (soft delete - cambia estado a inactivo).
        /// </summary>
        /// <returns>Confirmación de eliminación.</returns>
        /// <response code="200">Cuenta desactivada correctamente.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpDelete("perfil")]
        [Authorize(Roles = "Usuario")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> EliminarPerfil()
        {
            var idUsuarioToken = _usuarioActual.ObtenerUsuarioId();

            if (idUsuarioToken <= 0)
                return Unauthorized(new { error = "El token no incluye el perfil de usuario asociado." });

            var usuario = await _context.Usuarios
                .Include(u => u.Cuenta)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuarioToken);

            if (usuario == null)
                return NotFound(new { error = "Usuario no encontrado." });

            var cuenta = usuario.Cuenta;
            if (cuenta != null)
                cuenta.Estado = "inactivo";

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cuenta desactivada correctamente." });
        }

        // ============================================================
        // 4) REGLAS DE NEGOCIO PRIVADAS COMPARTIDAS
        // ============================================================
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

    /// <summary>
    /// Datos para actualizar el perfil del usuario autenticado.
    /// </summary>
    public class ActualizarPerfilUsuarioDto
    {
        /// <summary>Nuevo nombre del usuario (opcional).</summary>
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string? Nombre { get; set; }

        /// <summary>Nuevo correo de la cuenta (opcional).</summary>
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [MaxLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
        public string? Correo { get; set; }

        /// <summary>Nueva contraseña de la cuenta (opcional, mínimo 8 caracteres).</summary>
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(255, ErrorMessage = "La contraseña no puede exceder 255 caracteres.")]
        public string? Contrasena { get; set; }
    }
}