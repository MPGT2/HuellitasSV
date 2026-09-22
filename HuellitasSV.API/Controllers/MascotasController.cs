// [HU-10] Michael Menendez: Estructura base - Clase parcial raíz del controlador de mascotas.
// Contiene la ruta base, la inyección del contexto y la regla de negocio compartida.
// Los endpoints se agrupan por HU en archivos parciales: HU-04, HU-05, HU-06 y HU-09.

namespace HuellitasSV.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using HuellitasSV.API.Data;

/// <summary>
/// Controlador base (clase parcial) de mascotas. Define la ruta y el estado común;
/// los endpoints se agrupan por Historia de Usuario en los demás archivos parciales.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public partial class MascotasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicializa el controlador con el contexto de base de datos.
    /// </summary>
    public MascotasController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Regla de negocio de transición de estado:
    /// 'adoptada -> disponible' exige justificación; 'fallecida' es irreversible.
    /// </summary>
    private static bool EsTransicionEstadoValida(string estadoActual, string nuevoEstado, string? justificacion)
    {
        if (estadoActual == "adoptada" && nuevoEstado == "disponible" && string.IsNullOrWhiteSpace(justificacion))
            return false;
        if (estadoActual == "fallecida" && nuevoEstado != "fallecida")
            return false;
        return true;
    }
}