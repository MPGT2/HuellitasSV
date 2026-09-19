using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Controllers;

/// <summary>
/// Controlador REST para la gestión de usuarios del sistema HuellitasSV.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UsuariosController"/>.
    /// </summary>
    /// <param name="context">Contexto de base de datos inyectado.</param>
    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene el listado completo de usuarios registrados.
    /// </summary>
    /// <returns>Lista de usuarios en formato JSON.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        return Ok(usuarios);
    }

    /// <summary>
    /// Obtiene un usuario específico por su identificador.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <returns>El usuario encontrado o NotFound si no existe.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario is null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="usuario">Datos del usuario a registrar.</param>
    /// <returns>El usuario creado con su identificador asignado.</returns>
    [HttpPost]
    public async Task<ActionResult<Usuario>> PostUsuario([FromBody] Usuario usuario)
    {
        usuario.FechaRegistro = DateTime.UtcNow;
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, usuario);
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente.
    /// </summary>
    /// <param name="id">Identificador del usuario a actualizar.</param>
    /// <param name="usuario">Datos actualizados del usuario.</param>
    /// <returns>NoContent si la actualización es exitosa; BadRequest o NotFound en caso contrario.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutUsuario(int id, [FromBody] Usuario usuario)
    {
        if (id != usuario.IdUsuario)
        {
            return BadRequest("El identificador de la ruta no coincide con el del cuerpo de la solicitud.");
        }

        var existe = await _context.Usuarios.AnyAsync(u => u.IdUsuario == id);
        if (!existe)
        {
            return NotFound();
        }

        _context.Entry(usuario).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Elimina un usuario del sistema.
    /// </summary>
    /// <param name="id">Identificador del usuario a eliminar.</param>
    /// <returns>NoContent si se eliminó correctamente; NotFound si no existe.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario is null)
        {
            return NotFound();
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
