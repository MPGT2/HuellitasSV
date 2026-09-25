// [SEGURIDAD] Michael Menendez: Emisión de tokens JWT.
// Genera un token firmado (HMAC-SHA256) con los claims: sub (idCuenta), rol,
// idRefugio (para refugios), idUsuario (para usuarios) y nombre. La vigencia se
// configura en appsettings.json (Jwt:ExpiryMinutes).

namespace HuellitasSV.API.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Servicio que emite tokens JWT firmados para los logins de la API.
/// </summary>
public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Inicializa el servicio con la configuración de la sección "Jwt".
    /// </summary>
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Genera un token JWT para una cuenta autenticada.
    /// </summary>
    /// <param name="idCuenta">Identificador de la cuenta (claim "sub").</param>
    /// <param name="rol">Rol de la cuenta (claim de rol: Admin, Refugio o Usuario).</param>
    /// <param name="nombre">Nombre visible del titular (opcional).</param>
    /// <param name="idRefugio">Identificador del refugio asociado, si el rol es Refugio.</param>
    /// <param name="idUsuario">Identificador del perfil de usuario, si el rol es Usuario.</param>
    /// <returns>Token JWT firmado en formato compacto.</returns>
    public string GenerarToken(long idCuenta, string rol, string? nombre, long? idRefugio, long? idUsuario)
    {
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var minutosVigencia = 480;
        if (int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var minutosConfigurados))
        {
            minutosVigencia = minutosConfigurados;
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, idCuenta.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, rol)
        };

        if (!string.IsNullOrEmpty(nombre))
        {
            claims.Add(new(JwtRegisteredClaimNames.GivenName, nombre));
        }

        if (idRefugio.HasValue)
        {
            claims.Add(new("idRefugio", idRefugio.Value.ToString()));
        }

        if (idUsuario.HasValue)
        {
            claims.Add(new("idUsuario", idUsuario.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(minutosVigencia),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}