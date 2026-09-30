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
    /// Controlador que gestiona el registro, la autenticación y el ciclo de aprobación
    /// de los refugios de HuellitasSV (HU-02 y HU-24).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RefugiosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>Servicio de emisión de tokens JWT (seguridad).</summary>
        private readonly Security.JwtTokenService _tokenService;

        private readonly IPasswordService _passwords;
        private readonly IAccountStatePolicy _estadoCuenta;
        private readonly ICurrentUserService _usuarioActual;
        private readonly IArchivoService _archivos;
        private readonly ICalificacionService _calificaciones;

        /// <summary>
        /// Inicializa el controlador con el contexto de base de datos y los servicios compartidos.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework Core de HuellitasSV.</param>
        /// <param name="tokenService">Servicio de emisión de tokens JWT.</param>
        /// <param name="passwords">Servicio compartido de cifrado y verificación de contraseñas.</param>
        /// <param name="estadoCuenta">Regla compartida de estado de cuenta.</param>
        /// <param name="usuarioActual">Claims del token ya convertidos.</param>
        /// <param name="archivos">Servicio de guardado de archivos (documentación del refugio).</param>
        /// <param name="calificaciones">Resumen de calificaciones para el perfil público.</param>
        public RefugiosController(
            ApplicationDbContext context,
            Security.JwtTokenService tokenService,
            IPasswordService passwords,
            IAccountStatePolicy estadoCuenta,
            ICurrentUserService usuarioActual,
            IArchivoService archivos,
            ICalificacionService calificaciones)
        {
            _context = context;
            _tokenService = tokenService;
            _passwords = passwords;
            _estadoCuenta = estadoCuenta;
            _usuarioActual = usuarioActual;
            _archivos = archivos;
            _calificaciones = calificaciones;
        }

        // ============================================================
        // 1) MÉTODOS [HttpGet]
        // ============================================================

        /// <summary>
        /// [HU-02] Perfil público de un refugio: nombre, ubicación, contacto y calificación.
        /// Alimenta la vista del refugio en la app (no requiere autenticación).
        /// </summary>
        /// <param name="id">Identificador del refugio.</param>
        /// <returns>Datos públicos del refugio.</returns>
        /// <response code="200">Refugio encontrado.</response>
        /// <response code="404">Refugio no encontrado.</response>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(RefugioRespuestaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RefugioRespuestaDto>> GetRefugio(long id)
        {
            var refugio = await _context.Refugio
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRefugio == id);

            if (refugio is null)
                return NotFound(new { error = "Refugio no encontrado." });

            var resumen = await _calificaciones.ObtenerResumenAsync(new[] { refugio.IdRefugio });

            return Ok(new RefugioRespuestaDto
            {
                IdRefugio = refugio.IdRefugio,
                NombreOrganizacion = refugio.NombreOrganizacion,
                Departamento = refugio.Departamento,
                Municipio = refugio.Municipio,
                Contacto = refugio.Contacto,
                TotalCalificaciones = resumen.Total(refugio.IdRefugio),
                PromedioEstrellas = resumen.Promedio(refugio.IdRefugio)
            });
        }

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
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Obtiene el perfil del refugio autenticado.
        /// </summary>
        /// <returns>Perfil del refugio.</returns>
        /// <response code="200">Perfil del refugio.</response>
        /// <response code="404">Refugio no encontrado.</response>
        [HttpGet("perfil")]
        [Authorize(Roles = "Refugio")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetPerfil()
        {
            var idRefugioToken = _usuarioActual.ObtenerRefugioId();

            if (idRefugioToken <= 0)
                return Unauthorized(new { error = "El token no incluye el refugio asociado." });

            var refugio = await _context.Refugio
                .Include(r => r.Cuenta)
                .FirstOrDefaultAsync(r => r.IdRefugio == idRefugioToken);

            if (refugio == null)
                return NotFound(new { error = "Refugio no encontrado." });

            return Ok(new
            {
                refugio.IdRefugio,
                refugio.NombreOrganizacion,
                refugio.Departamento,
                refugio.Municipio,
                refugio.Contacto,
                refugio.DocumentacionUrl,
                refugio.Latitud,
                refugio.Longitud,
                refugio.EstadoAprobacion,
                Correo = refugio.Cuenta?.Correo,
                Rol = refugio.Cuenta?.Rol,
                EstadoCuenta = refugio.Cuenta?.Estado
            });
        }

        /// <summary>
        /// Lista todos los refugios (solo para administradores).
        /// </summary>
        /// <returns>Lista de todos los refugios.</returns>
        /// <response code="200">Lista de refugios.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Refugio>>> GetAll()
        {
            var refugios = await _context.Refugio
                .Include(r => r.Cuenta)
                .OrderBy(r => r.NombreOrganizacion)
                .ToListAsync();

            return Ok(refugios);
        }

        /// <summary>
        /// Actualiza el perfil del refugio autenticado.
        /// </summary>
        /// <param name="dto">Datos a actualizar.</param>
        /// <returns>Perfil actualizado.</returns>
        /// <response code="200">Perfil actualizado correctamente.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="404">Refugio no encontrado.</response>
        [HttpPut("perfil")]
        [Authorize(Roles = "Refugio")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ActualizarPerfil(ActualizarPerfilRefugioDto dto)
        {
            var idRefugioToken = _usuarioActual.ObtenerRefugioId();

            if (idRefugioToken <= 0)
                return Unauthorized(new { error = "El token no incluye el refugio asociado." });

            var refugio = await _context.Refugio
                .Include(r => r.Cuenta)
                .FirstOrDefaultAsync(r => r.IdRefugio == idRefugioToken);

            if (refugio == null)
                return NotFound(new { error = "Refugio no encontrado." });

            if (!string.IsNullOrEmpty(dto.NombreOrganizacion))
            {
                var nombre = dto.NombreOrganizacion.Trim();
                var existeNombre = await _context.Refugio.AnyAsync(r => r.NombreOrganizacion == nombre && r.IdRefugio != refugio.IdRefugio);
                if (existeNombre)
                    return BadRequest(new { error = "El nombre de la organización ya está registrado." });

                refugio.NombreOrganizacion = nombre;
            }

            if (!string.IsNullOrEmpty(dto.Departamento))
                refugio.Departamento = dto.Departamento.Trim();

            if (!string.IsNullOrEmpty(dto.Municipio))
                refugio.Municipio = dto.Municipio.Trim();

            if (!string.IsNullOrEmpty(dto.Contacto))
                refugio.Contacto = dto.Contacto.Trim();

            if (dto.Latitud.HasValue)
            {
                if (dto.Latitud < -90 || dto.Latitud > 90)
                    return BadRequest(new { error = "La latitud debe estar entre -90 y 90." });
                refugio.Latitud = dto.Latitud;
            }

            if (dto.Longitud.HasValue)
            {
                if (dto.Longitud < -180 || dto.Longitud > 180)
                    return BadRequest(new { error = "La longitud debe estar entre -180 y 180." });
                refugio.Longitud = dto.Longitud;
            }

            if (!string.IsNullOrEmpty(dto.DocumentacionUrl))
                refugio.DocumentacionUrl = dto.DocumentacionUrl;

            var cuenta = refugio.Cuenta;
            if (cuenta != null && !string.IsNullOrEmpty(dto.Correo))
            {
                var nuevoCorreo = dto.Correo.Trim().ToLowerInvariant();
                var existeCorreo = await _context.Cuenta.AnyAsync(c => c.Correo == nuevoCorreo && c.IdCuenta != cuenta.IdCuenta);
                if (existeCorreo)
                    return BadRequest(new { error = "El correo ya está registrado por otra cuenta." });

                cuenta.Correo = nuevoCorreo;
            }

            if (cuenta != null && !string.IsNullOrEmpty(dto.Contrasena))
            {
                if (dto.Contrasena.Length < 8)
                    return BadRequest(new { error = "La contraseña debe tener al menos 8 caracteres." });

                cuenta.Contrasena = _passwords.Hash(cuenta, dto.Contrasena);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Perfil actualizado correctamente.",
                refugio.IdRefugio,
                refugio.NombreOrganizacion,
                refugio.Departamento,
                refugio.Municipio,
                refugio.Contacto,
                refugio.DocumentacionUrl,
                refugio.Latitud,
                refugio.Longitud,
                refugio.EstadoAprobacion,
                Correo = cuenta?.Correo
            });
        }

        /// <summary>
        /// Desactiva el refugio autenticado (soft delete - cambia estado de la cuenta a inactivo).
        /// </summary>
        /// <returns>Confirmación de desactivación.</returns>
        /// <response code="200">Refugio desactivado correctamente.</response>
        /// <response code="404">Refugio no encontrado.</response>
        [HttpDelete("perfil")]
        [Authorize(Roles = "Refugio")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> EliminarPerfil()
        {
            var idRefugioToken = _usuarioActual.ObtenerRefugioId();

            if (idRefugioToken <= 0)
                return Unauthorized(new { error = "El token no incluye el refugio asociado." });

            var refugio = await _context.Refugio
                .Include(r => r.Cuenta)
                .FirstOrDefaultAsync(r => r.IdRefugio == idRefugioToken);

            if (refugio == null)
                return NotFound(new { error = "Refugio no encontrado." });

            var cuenta = refugio.Cuenta;
            if (cuenta != null)
                cuenta.Estado = "inactivo";

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Refugio desactivado correctamente." });
        }

        // ============================================================
        // 2) MÉTODOS [HttpPost]
        // ============================================================

        /// <summary>
        /// [HU-02] Registra un nuevo refugio: valida los campos obligatorios, que el correo
        /// y el nombre de la organización no existan, crea la cuenta con rol "Refugio"
        /// y deja el refugio en estado "pendiente" a la espera de aprobación (HU-24).
        /// </summary>
        /// <param name="dto">Datos del refugio a registrar (multipart/form-data; incluye el documento opcional).</param>
        /// <returns>Confirmación del registro con el identificador generado.</returns>
        /// <response code="201">Refugio registrado correctamente (queda pendiente de aprobación).</response>
        /// <response code="400">Si los campos obligatorios no son válidos o el documento no cumple las reglas.</response>
        /// <response code="409">Si el correo o el nombre de la organización ya están registrados.</response>
        [HttpPost("registro")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ActionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> RegistrarRefugio([FromForm] RegistroRefugioDto dto)
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

            // Documentación de respaldo (opcional): se valida y guarda antes de persistir el refugio.
            string? documentacionUrl;
            try
            {
                documentacionUrl = await _archivos.GuardarAsync(dto.Documentacion, "refugios");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            // HU-02: se crea la cuenta con rol "Refugio" y estado "pendiente" (contraseña con hash PBKDF2).
            var cuenta = new Cuenta
            {
                Correo = correo,
                Contrasena = _passwords.Hash(dto.Contrasena),
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
                DocumentacionUrl = documentacionUrl,
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
            if (cuenta == null || cuenta.Rol != "Refugio" || !await _passwords.VerificarAsync(cuenta, dto.Contrasena))
            {
                return Unauthorized(new { error = "Credenciales incorrectas." });
            }

            // Una cuenta desactivada o bloqueada no entra, aunque el refugio siga aprobado.
            // Esta comprobación faltaba y la hacía el login unificado; sin ella, un refugio
            // desactivado seguía entrando por este endpoint heredado.
            if (_estadoCuenta.EstaBloqueada(cuenta))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = _estadoCuenta.ConstruirMensajeBloqueo(cuenta)
                });
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
                token = _tokenService.GenerarToken(cuenta.IdCuenta, cuenta.Rol, refugio.NombreOrganizacion, refugio.IdRefugio, null),
                idRefugio = refugio.IdRefugio,
                idCuenta = cuenta.IdCuenta,
                nombreOrganizacion = refugio.NombreOrganizacion,
                rol = cuenta.Rol
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
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Documentación de respaldo del refugio (acta o permiso, PDF/JPG), opcional.
        /// Se sube como archivo dentro del mismo multipart/form-data bajo el campo "Documentacion".
        /// </summary>
        public IFormFile? Documentacion { get; set; }
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

    /// <summary>
    /// Datos para actualizar el perfil del refugio autenticado.
    /// </summary>
    public class ActualizarPerfilRefugioDto
    {
        /// <summary>Nuevo nombre de la organización (opcional).</summary>
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
        public string? NombreOrganizacion { get; set; }

        /// <summary>Nuevo departamento (opcional).</summary>
        [MaxLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres.")]
        public string? Departamento { get; set; }

        /// <summary>Nuevo municipio (opcional).</summary>
        [MaxLength(100, ErrorMessage = "El municipio no puede exceder 100 caracteres.")]
        public string? Municipio { get; set; }

        /// <summary>Nueva información de contacto (opcional).</summary>
        [MaxLength(150, ErrorMessage = "El contacto no puede exceder 150 caracteres.")]
        public string? Contacto { get; set; }

        /// <summary>Nueva URL de documentación (opcional).</summary>
        [MaxLength(255, ErrorMessage = "La URL no puede exceder 255 caracteres.")]
        public string? DocumentacionUrl { get; set; }

        /// <summary>Nueva latitud (opcional).</summary>
        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
        public double? Latitud { get; set; }

        /// <summary>Nueva longitud (opcional).</summary>
        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
        public double? Longitud { get; set; }

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
