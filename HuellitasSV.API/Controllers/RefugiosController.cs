// [HU-02 / HU-24] Michael Menendez: Controlador de refugios.
// HU-02: Registro de refugio (crea cuenta con rol "Refugio" y estado "pendiente") e inicio de sesión.
// HU-24: Aprobación y control de refugios por el administrador (listar pendientes, aprobar/rechazar).
// Nota: los valores de estado se manejan en minúsculas (pendiente/aprobado/rechazado),
// igual que los datos semilla de la base de datos.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using HuellitasSV.API.Data;
using HuellitasSV.API.Models;

namespace HuellitasSV.API.Controllers
{
    /// <summary>
    /// Controlador que gestiona el registro, la autenticación y el ciclo de aprobación
    /// de los refugios de HuellitasSV (HU-02 y HU-24).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RefugiosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>Componente de hash de contraseñas (PBKDF2, sin estado, seguro en hilos).</summary>
        private static readonly PasswordHasher<Cuenta> _hasher = new();

        /// <summary>
        /// Inicializa el controlador con el contexto de base de datos inyectado.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework Core de HuellitasSV.</param>
        public RefugiosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1) MÉTODOS [HttpGet]
        // ============================================================

        /// <summary>
        /// [HU-09] Devuelve las mascotas de un refugio, con filtro opcional por estado.
        /// Pensado para el panel del refugio: si no se envía el parámetro, devuelve todas.
        /// </summary>
        /// <param name="id">Identificador del refugio.</param>
        /// <param name="estado">Estado de la mascota (disponible, adoptada, etc.). Opcional.</param>
        /// <returns>Lista de mascotas del refugio en formato JSON.</returns>
        /// <response code="200">Devuelve la lista de mascotas del refugio.</response>
        /// <response code="404">Si el refugio no existe.</response>
        [HttpGet("{id}/mascotas")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetMascotas(long id, [FromQuery] string? estado)
        {
            var refugio = await _context.Refugio.FindAsync(id);
            if (refugio == null)
            {
                return NotFound(new { error = "Refugio no encontrado." });
            }

            var query = _context.Mascota.Where(m => m.IdRefugio == id).AsQueryable();
            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(m => m.Estado == estado.Trim().ToLowerInvariant());
            }

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
        /// [HU-24] Obtiene la lista de refugios cuyo estado de aprobación sea "pendiente",
        /// para que el administrador pueda revisarlos y aprobarlos o rechazarlos.
        /// </summary>
        /// <returns>Lista de refugios pendientes de aprobación en formato JSON.</returns>
        /// <response code="200">Devuelve la lista de refugios pendientes.</response>
        [HttpGet("pendientes")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Refugio>>> GetPendientes()
        {
            var refugios = await _context.Refugio
                .AsNoTracking()
                .Where(r => r.EstadoAprobacion == "pendiente")
                .OrderBy(r => r.NombreOrganizacion)
                .ToListAsync();

            return Ok(refugios);
        }

        // ============================================================
        // 2) MÉTODOS [HttpPost]
        // ============================================================

        /// <summary>
        /// [HU-02] Registra un nuevo refugio: valida los campos obligatorios, que el correo
        /// y el nombre de la organización no existan, crea la cuenta con rol "Refugio"
        /// y deja el refugio en estado "pendiente" a la espera de aprobación (HU-24).
        /// </summary>
        /// <param name="dto">Datos del refugio a registrar (JSON).</param>
        /// <returns>Confirmación del registro con el identificador generado.</returns>
        /// <response code="201">Refugio registrado correctamente (queda pendiente de aprobación).</response>
        /// <response code="400">Si los campos obligatorios no son válidos.</response>
        /// <response code="409">Si el correo o el nombre de la organización ya están registrados.</response>
        [HttpPost("registro")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> RegistrarRefugio(RegistroRefugioDto dto)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();
            var nombre = dto.NombreOrganizacion.Trim();

            // Regla de negocio HU-02: el correo de la cuenta no debe existir.
            var existeCorreo = await _context.Cuenta.AnyAsync(c => c.Correo == correo);
            if (existeCorreo)
            {
                return Conflict(new { error = "El correo ya está registrado en el sistema." });
            }

            // Regla de negocio HU-02: el nombre de la organización no debe existir.
            var existeNombre = await _context.Refugio.AnyAsync(r => r.NombreOrganizacion == nombre);
            if (existeNombre)
            {
                return Conflict(new { error = "El nombre de la organización ya está registrado." });
            }

            // HU-02: se crea la cuenta con rol "Refugio" y estado "pendiente" (contraseña con hash PBKDF2).
            var cuenta = new Cuenta
            {
                Correo = correo,
                Contrasena = _hasher.HashPassword(null!, dto.Contrasena),
                Rol = "Refugio",
                Estado = "pendiente"
            };
            _context.Cuenta.Add(cuenta);
            await _context.SaveChangesAsync();

            // El refugio queda vinculado a su cuenta y en espera de aprobación (HU-24).
            var refugio = new Refugio
            {
                IdCuenta = cuenta.IdCuenta,
                NombreOrganizacion = nombre,
                Departamento = dto.Departamento.Trim(),
                Municipio = dto.Municipio.Trim(),
                Contacto = dto.Contacto.Trim(),
                EstadoAprobacion = "pendiente"
            };
            _context.Refugio.Add(refugio);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new
            {
                mensaje = "Refugio registrado correctamente. En espera de aprobación del administrador.",
                idRefugio = refugio.IdRefugio,
                idCuenta = cuenta.IdCuenta,
                nombreOrganizacion = refugio.NombreOrganizacion,
                correo = cuenta.Correo,
                rol = cuenta.Rol,
                estadoAprobacion = refugio.EstadoAprobacion
            });
        }

        /// <summary>
        /// [HU-02] Inicia sesión para un refugio validando sus credenciales (correo y contraseña)
        /// y su estado de aprobación: solo los refugios aprobados pueden acceder.
        /// </summary>
        /// <param name="dto">Credenciales de la cuenta del refugio (JSON).</param>
        /// <returns>Confirmación de autenticación con el identificador del refugio.</returns>
        /// <response code="200">Autenticación exitosa.</response>
        /// <response code="401">Si las credenciales son incorrectas.</response>
        /// <response code="403">Si la cuenta existe pero el refugio no está aprobado.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> Login(LoginRefugioDto dto)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            var cuenta = await _context.Cuenta.FirstOrDefaultAsync(c => c.Correo == correo);
            if (cuenta == null || cuenta.Rol != "Refugio" || !await VerificarContrasenaAsync(cuenta, dto.Contrasena))
            {
                return Unauthorized(new { error = "Credenciales incorrectas." });
            }

            var refugio = await _context.Refugio.FirstOrDefaultAsync(r => r.IdCuenta == cuenta.IdCuenta);
            if (refugio == null)
            {
                return NotFound(new { error = "La cuenta no tiene un refugio asociado." });
            }

            // Regla de negocio HU-02: solo refugios aprobados pueden iniciar sesión.
            if (refugio.EstadoAprobacion != "aprobado")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = $"Acceso denegado. El estado actual de su cuenta es: {refugio.EstadoAprobacion}."
                });
            }

            return Ok(new
            {
                mensaje = "Autenticación exitosa.",
                idRefugio = refugio.IdRefugio,
                nombreOrganizacion = refugio.NombreOrganizacion
            });
        }

        // ============================================================
        // 3) MÉTODOS [HttpPut]
        // ============================================================

        /// <summary>
        /// [HU-24] Aprueba o rechaza un refugio cambiando su estado de aprobación.
        /// El estado recibido se normaliza y solo se admite "aprobado" o "rechazado".
        /// </summary>
        /// <param name="id">Identificador del refugio.</param>
        /// <param name="dto">DTO con el nuevo estado ("aprobado" o "rechazado").</param>
        /// <returns>Confirmación del cambio de estado en formato JSON.</returns>
        /// <response code="200">Estado actualizado correctamente.</response>
        /// <response code="400">Si el estado indicado no es válido.</response>
        /// <response code="404">Si el refugio no existe.</response>
        [HttpPut("{id}/estado")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CambiarEstado(long id, CambiarEstadoRefugioDto dto)
        {
            var nuevoEstado = dto.Estado?.Trim().ToLowerInvariant() ?? string.Empty;
            if (nuevoEstado != "aprobado" && nuevoEstado != "rechazado")
            {
                return BadRequest(new { error = "Estado no válido. Solo se admite 'aprobado' o 'rechazado'." });
            }

            var refugio = await _context.Refugio.FindAsync(id);
            if (refugio == null)
            {
                return NotFound(new { error = "Refugio no encontrado." });
            }

            refugio.EstadoAprobacion = nuevoEstado;

            // Se mantiene sincronizada la cuenta asociada (la conexión refugio -> cuenta).
            var cuenta = await _context.Cuenta.FindAsync(refugio.IdCuenta);
            if (cuenta != null)
            {
                cuenta.Estado = nuevoEstado;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = $"El estado del refugio se actualizó a: {nuevoEstado}.",
                idRefugio = refugio.IdRefugio,
                nombreOrganizacion = refugio.NombreOrganizacion,
                estadoAprobacion = refugio.EstadoAprobacion
            });
        }

        // ============================================================
        // REGLA DE NEGOCIO PRIVADA COMPARTIDA
        // ============================================================

        /// <summary>
        /// Verifica una contraseña contra el hash almacenado (PBKDF2). Da soporte de
        /// migración progresiva: las cuentas creadas antes del hashing (contraseña en
        /// texto plano) se convierten automáticamente a hash al iniciar sesión.
        /// </summary>
        /// <param name="cuenta">Cuenta cuya contraseña se verifica.</param>
        /// <param name="contrasena">Contraseña proporcionada por el refugio.</param>
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
    /// Datos requeridos para registrar un nuevo refugio (HU-02).
    /// </summary>
    public class RegistroRefugioDto
    {
        /// <summary>Nombre de la organización del refugio.</summary>
        [Required]
        public string NombreOrganizacion { get; set; } = string.Empty;

        /// <summary>Correo de la cuenta del refugio (debe ser único).</summary>
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        /// <summary>Contraseña de la cuenta del refugio.</summary>
        [Required]
        [MinLength(8)]
        public string Contrasena { get; set; } = string.Empty;

        /// <summary>Departamento donde se ubica el refugio.</summary>
        [Required]
        public string Departamento { get; set; } = string.Empty;

        /// <summary>Municipio donde se ubica el refugio.</summary>
        [Required]
        public string Municipio { get; set; } = string.Empty;

        /// <summary>Información de contacto del refugio.</summary>
        [Required]
        public string Contacto { get; set; } = string.Empty;
    }

    /// <summary>
    /// Credenciales para el inicio de sesión de un refugio (HU-02).
    /// </summary>
    public class LoginRefugioDto
    {
        /// <summary>Correo de la cuenta del refugio.</summary>
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        /// <summary>Contraseña de la cuenta del refugio.</summary>
        [Required]
        public string Contrasena { get; set; } = string.Empty;
    }

    /// <summary>
    /// Nuevo estado de aprobación para un refugio (HU-24).
    /// </summary>
    public class CambiarEstadoRefugioDto
    {
        /// <summary>Nuevo estado: "aprobado" o "rechazado".</summary>
        [Required]
        public string Estado { get; set; } = string.Empty;
    }
}