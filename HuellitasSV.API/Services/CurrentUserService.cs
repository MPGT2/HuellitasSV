using System.Globalization;
using System.Security.Claims;

namespace HuellitasSV.API.Services;

/// <summary>
/// Lee una sola vez los claims del token y los expone ya convertidos.
/// El patrón <c>long.TryParse(User.FindFirstValue("idRefugio"), out var x) ? x : 0</c>
/// estaba copiado 13 veces en 8 controladores, y de esas comparaciones salía la
/// decisión de autorización. Aquí queda en un solo lugar.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Claim "sub": identificador de la cuenta.</summary>
    long? IdCuenta { get; }

    /// <summary>Rol con el que se autenticó: Admin, Refugio o Usuario.</summary>
    string? Rol { get; }

    /// <summary>Identificador del refugio, o null si el token no lo trae.</summary>
    long? RefugioId { get; }

    /// <summary>Identificador del usuario, o null si el token no lo trae.</summary>
    long? UsuarioId { get; }

    bool EsRefugio { get; }

    bool EsUsuario { get; }

    /// <summary>
    /// Identificador del refugio del token, o <paramref name="porDefecto"/> si no viene.
    /// </summary>
    long ObtenerRefugioId(long porDefecto = 0);

    /// <summary>
    /// Identificador del usuario del token, o <paramref name="porDefecto"/> si no viene.
    /// </summary>
    long ObtenerUsuarioId(long porDefecto = 0);
}

/// <inheritdoc cref="ICurrentUserService"/>
public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User;
    }

    public long? IdCuenta => LeerNumero(JwtRegisteredNameSub);

    public string? Rol => _principal?.FindFirst(ClaimTypes.Role)?.Value;

    public long? RefugioId => LeerNumero("idRefugio");

    public long? UsuarioId => LeerNumero("idUsuario");

    public bool EsRefugio => Rol == "Refugio";

    public bool EsUsuario => Rol == "Usuario";

    public long ObtenerRefugioId(long porDefecto = 0) => RefugioId ?? porDefecto;

    public long ObtenerUsuarioId(long porDefecto = 0) => UsuarioId ?? porDefecto;

    private const string JwtRegisteredNameSub = "sub";

    /// <summary>
    /// Convierte un claim a número. Devuelve null si el claim no existe, está vacío
    /// o no es un entero, que es lo mismo que antes de esta clase devolvía 0.
    /// </summary>
    private long? LeerNumero(string claim)
    {
        var valor = _principal?.FindFirst(claim)?.Value;
        if (string.IsNullOrEmpty(valor))
            return null;

        return long.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero)
            ? numero
            : null;
    }
}
