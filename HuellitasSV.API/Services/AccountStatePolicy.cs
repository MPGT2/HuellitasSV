using HuellitasSV.API.Models;

namespace HuellitasSV.API.Services;

/// <summary>
/// Regla de negocio compartida sobre el estado de una cuenta.
/// La regla "inactivo o bloqueado no entra" estaba escrita a mano en el
/// login unificado y en el login heredado de usuarios, con el mismo mensaje.
/// Aquí se decide una sola vez.
/// </summary>
public interface IAccountStatePolicy
{
    /// <summary>Indica si el estado de la cuenta impide el acceso.</summary>
    bool EstaBloqueada(Cuenta cuenta);

    /// <summary>Devuelve el mensaje 403 que ya devolvían los controladores.</summary>
    string ConstruirMensajeBloqueo(Cuenta cuenta);
}

/// <inheritdoc cref="IAccountStatePolicy"/>
public class AccountStatePolicy : IAccountStatePolicy
{
    public bool EstaBloqueada(Cuenta cuenta) =>
        cuenta.Estado == "inactivo" || cuenta.Estado == "bloqueado";

    public string ConstruirMensajeBloqueo(Cuenta cuenta)
    {
        var motivo = cuenta.Estado == "inactivo" ? "inactiva" : "bloqueada";
        return $"Acceso denegado: su cuenta está {motivo}. Contacte al administrador.";
    }
}
