using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HuellitasSV.API.Services;

/// <summary>
/// Única fuente de verdad para guardar archivos subidos (imágenes y documentos)
/// en wwwroot/imagenes. Devuelve siempre una ruta relativa, que es la que se
/// persiste en la base de datos y la que <c>app.UseStaticFiles()</c> sirve.
/// </summary>
public interface IArchivoService
{
    /// <summary>
    /// Guarda el archivo en wwwroot/imagenes/<paramref name="carpeta"/> con un nombre
    /// único (Guid) y devuelve su ruta relativa ("/imagenes/carpeta/archivo.ext").
    /// Devuelve null si no se recibió archivo.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Si el archivo supera los 5 MB o su extensión no está permitida.
    /// </exception>
    Task<string?> GuardarAsync(IFormFile? archivo, string carpeta, CancellationToken ct = default);
}

/// <inheritdoc />
public class ArchivoService : IArchivoService
{
    /// <summary>Tamaño máximo permitido por archivo: 10 MB.</summary>
    private const long MaxBytes = 10 * 1024 * 1024;

    /// <summary>Extensiones admitidas: imágenes comunes y PDF para documentación.</summary>
    private static readonly string[] ExtensionesPermitidas =
        { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf" };

    private readonly IWebHostEnvironment _env;

    /// <summary>Inicializa el servicio con el entorno web para resolver wwwroot.</summary>
    public ArchivoService(IWebHostEnvironment env) => _env = env;

    /// <inheritdoc />
    public async Task<string?> GuardarAsync(IFormFile? archivo, string carpeta, CancellationToken ct = default)
    {
        if (archivo is null || archivo.Length == 0)
            return null;

        if (archivo.Length > MaxBytes)
            throw new InvalidOperationException("El archivo supera el tamaño máximo permitido de 10 MB.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Extensión no permitida ({extension}). Se admiten: {string.Join(", ", ExtensionesPermitidas)}.");
        }

        // wwwroot puede venir nulo en entornos de prueba; se cae a ContentRoot/wwwroot.
        var raiz = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var destino = Path.Combine(raiz, "imagenes", carpeta);
        Directory.CreateDirectory(destino);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaFisica = Path.Combine(destino, nombreArchivo);

        await using (var stream = new FileStream(rutaFisica, FileMode.Create))
        {
            await archivo.CopyToAsync(stream, ct);
        }

        // Ruta relativa lista para la BD y para resolveImageUrl() en el móvil.
        return $"/imagenes/{carpeta}/{nombreArchivo}";
    }
}
