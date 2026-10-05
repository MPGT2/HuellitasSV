export const CLAVE_SESION = "huellitas_sesion";

export function leerSesion() {
  try {
    return JSON.parse(localStorage.getItem(CLAVE_SESION)) ?? null;
  } catch {
    return null;
  }
}
