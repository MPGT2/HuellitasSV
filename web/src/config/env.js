// URL pública de la API desplegada en MonsterASP (la misma que usa la app móvil).
// El segmento "/api" es obligatorio: los controladores usan [Route("api/[controller]")].
const API_DESPLEGADA = "https://huellitassv.runasp.net/api";

// Acepta la URL con o sin "/api" al final y siempre devuelve una que lo tenga.
function normalizar(url) {
  const limpia = String(url ?? "").trim().replace(/\/+$/, "");
  if (!limpia) return null;
  return /\/api$/i.test(limpia) ? limpia : `${limpia}/api`;
}

export const API_URL = normalizar(import.meta.env.VITE_API_URL) ?? API_DESPLEGADA;

// Origen de la API sin "/api". Las imágenes y documentos se guardan como ruta
// relativa ("/imagenes/...") y la API los sirve desde la raíz, no bajo /api.
export const API_ORIGIN = API_URL.replace(/\/api$/i, "");

// Convierte la ruta que guarda la BD en una URL que <img> puede cargar.
export function resolveImageUrl(url) {
  if (!url) return null;
  if (/^https?:\/\//i.test(url)) return url;
  return `${API_ORIGIN}${url.startsWith("/") ? "" : "/"}${url}`;
}
