// URL publica del backend desplegado en MonsterASP. Ojo: el dominio del sitio
// es "huellitassv.runasp.net"; "site95978.siteasp.net" es el servidor de
// alojamiento y su "/" solo sirve la pagina de parking (no la aplicacion).
// El segmento "/api" es obligatorio: los controladores usan
// [Route("api/[controller]")] y los servicios del movil llaman a rutas como
// "/Auth/login" (sin "/api").
const MONSTERASP_URL = 'https://huellitassv.runasp.net/api';

/**
 * Normaliza una URL base para que siempre termine en "/api".
 * Evita el error 404 cuando se define EXPO_PUBLIC_API_URL con el dominio
 * pelado (por ejemplo "https://huellitassv.runasp.net").
 */
function normalizeApiBaseUrl(url) {
  const trimmed = String(url).trim().replace(/\/+$/, '');
  if (!trimmed) return null;
  return /\/api$/i.test(trimmed) ? trimmed : `${trimmed}/api`;
}

// La app siempre consume el backend publico de MonsterASP. La deteccion automatica
// de la IP local se elimino a proposito: en Expo Go el hostUri siempre existe,
// hacia que la app apuntara a http://<ip-local>:5299/api en vez de a la API
// desplegada, que es justamente lo que hay que probar antes de entregar.
// Para Developing contra la API local se define EXPO_PUBLIC_API_URL a mano.
const fromEnv = normalizeApiBaseUrl(process.env.EXPO_PUBLIC_API_URL || '');

export const API_BASE_URL = fromEnv || normalizeApiBaseUrl(MONSTERASP_URL);

// Origen de la API sin el segmento "/api". Las imagenes y documentos se
// guardan como ruta relativa ("/imagenes/...") y las sirve UseStaticFiles()
// desde la raiz, no bajo /api.
export const API_ORIGIN = API_BASE_URL.replace(/\/api\/?$/, '');

// Como se resolvio la URL: 'env' si se sobrescribio, 'monsterasp' si se uso la
// URL desplegada. Se muestra en el panel de diagnostico de la app.
export const API_RESOLUTION = fromEnv ? 'env' : 'monsterasp';

/**
 * Convierte la ruta relativa que guarda la BD en una URL que <Image> puede cargar.
 * - Si ya es absoluta (http/https), la devuelve tal cual.
 * - Si viene vacia o nula, devuelve null.
 * - Si es relativa, le antepone el origen de la API.
 */
export function resolveImageUrl(url) {
  if (!url) return null;
  if (/^https?:\/\//i.test(url)) return url;
  return `${API_ORIGIN}${url.startsWith('/') ? '' : '/'}${url}`;
}