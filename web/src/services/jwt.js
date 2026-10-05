// Lee el rol que viene dentro del token JWT (si la API lo incluye).
// Devuelve el rol en minúsculas, o "" si no hay.
export function leerRol(token) {
  try {
    const payload = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    const datos = JSON.parse(atob(payload));
    const rol =
      datos.role ??
      datos.rol ??
      datos["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
      "";
    return String(Array.isArray(rol) ? rol.join(",") : rol).toLowerCase();
  } catch {
    return "";
  }
}
