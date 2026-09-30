using HuellitasSV.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Services;

/// <summary>Cantidad de calificaciones y promedio de estrellas de un refugio.</summary>
public sealed record ResumenCalificacion(long IdRefugio, int Total, double? Promedio);

/// <summary>
/// Resumen de calificaciones de varios refugios, consultado en una sola vuelta.
/// Se usa como fuente de datos en las proyecciones del catálogo para no repetir
/// la cuenta y el promedio dentro de cada mascota.
/// </summary>
public sealed class ResumenCalificaciones
{
    private readonly Dictionary<long, ResumenCalificacion> _porRefugio;

    public ResumenCalificaciones(IEnumerable<ResumenCalificacion> items)
    {
        _porRefugio = items.ToDictionary(i => i.IdRefugio);
    }

    /// <summary>Número de calificaciones del refugio; 0 si no tiene ninguna.</summary>
    public int Total(long idRefugio) =>
        _porRefugio.TryGetValue(idRefugio, out var resumen) ? resumen.Total : 0;

    /// <summary>Promedio de estrellas; null si el refugio no tiene calificaciones.</summary>
    public double? Promedio(long idRefugio) =>
        _porRefugio.TryGetValue(idRefugio, out var resumen) ? resumen.Promedio : null;
}

/// <summary>
/// Calcula la calificación de los refugios.
/// Antes el catálogo, el detalle y el filtro por ubicación repetían la misma
/// consulta, y como vivía dentro de la proyección se disparaba dos veces por
/// mascota. Resolviéndolo de forma aparte es una sola consulta por respuesta.
/// </summary>
public interface ICalificacionService
{
    Task<ResumenCalificaciones> ObtenerResumenAsync(IEnumerable<long> idsRefugio);
}

/// <inheritdoc cref="ICalificacionService"/>
public class CalificacionService : ICalificacionService
{
    private readonly ApplicationDbContext _context;

    public CalificacionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResumenCalificaciones> ObtenerResumenAsync(IEnumerable<long> idsRefugio)
    {
        var ids = idsRefugio.Distinct().ToList();
        if (ids.Count == 0)
            return new ResumenCalificaciones(Array.Empty<ResumenCalificacion>());

        var resumenes = await _context.Calificaciones
            .Where(c => ids.Contains(c.IdRefugio))
            .GroupBy(c => c.IdRefugio)
            .Select(g => new ResumenCalificacion(
                g.Key,
                g.Count(),
                g.Average(c => (double?)c.Estrellas)))
            .ToListAsync();

        return new ResumenCalificaciones(resumenes);
    }
}
