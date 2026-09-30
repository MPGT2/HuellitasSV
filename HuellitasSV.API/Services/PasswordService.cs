using HuellitasSV.API.Data;
using HuellitasSV.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HuellitasSV.API.Services;

/// <summary>
/// Única fuente de verdad para cifrar y verificar contraseñas.
/// Antes de esta clase cada controlador tenía su propia copia del
/// <see cref="PasswordHasher{TUser}"/>, con tres variantes distintas de la misma
/// verificación. Aquí vive una sola versión.
/// </summary>
public interface IPasswordService
{
    /// <summary>Genera el hash PBKDF2 de <paramref name="contrasena"/> para la cuenta.</summary>
    string Hash(Cuenta cuenta, string contrasena);

    /// <summary>
    /// Genera el hash de una contraseña para una cuenta que todavía no se guarda.
    /// </summary>
    string Hash(string contrasena);

    /// <summary>
    /// Comprueba la contraseña contra el hash almacenado y, si hace falta,
    /// reescribe la contraseña en un formato más seguro.
    /// </summary>
    Task<bool> VerificarAsync(Cuenta cuenta, string contrasena);
}

/// <inheritdoc cref="IPasswordService"/>
public class PasswordService : IPasswordService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<Cuenta> _hasher = new();

    public PasswordService(ApplicationDbContext context)
    {
        _context = context;
    }

    public string Hash(Cuenta cuenta, string contrasena) => _hasher.HashPassword(cuenta, contrasena);

    public string Hash(string contrasena) => _hasher.HashPassword(null!, contrasena);

    public async Task<bool> VerificarAsync(Cuenta cuenta, string contrasena)
    {
        PasswordVerificationResult resultado;
        try
        {
            resultado = _hasher.VerifyHashedPassword(cuenta, cuenta.Contrasena, contrasena);
        }
        catch (FormatException)
        {
            // Lo almacenado no tiene forma de hash: es una cuenta legada en texto plano.
            resultado = PasswordVerificationResult.Failed;
        }

        if (resultado == PasswordVerificationResult.Success)
            return true;

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            cuenta.Contrasena = Hash(cuenta, contrasena);
            await _context.SaveChangesAsync();
            return true;
        }

        // Cuenta legada: se compara el texto plano y se aprovecha para migrarla a hash.
        // La comparación no es de tiempo constante, pero solo se alcanza cuando el valor
        // guardado no es un hash, y la escritura siguiente la deja cifrada para siempre.
        if (cuenta.Contrasena == contrasena)
        {
            cuenta.Contrasena = Hash(cuenta, contrasena);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
