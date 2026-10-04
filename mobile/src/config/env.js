import Constants from 'expo-constants';
import { Platform } from 'react-native';

// URL de Render con puerto correcto (según logs de despliegue: "Now listening on: http://0.0.0.0:10000")
const RENDER_URL = 'https://huellitassv.onrender.com';

// Usar variable de entorno si se define (para builds personalizados),
// de lo contrario usar la URL de Render por defecto
const apiBaseUrl = process.env.EXPO_PUBLIC_API_URL || RENDER_URL;

export const API_BASE_URL = apiBaseUrl;

// Origen de la API sin el segmento "/api". Las imagenes y documentos se
// guardan como ruta relativa ("/imagenes/...") y las sirve UseStaticFiles()
// desde la raiz, no bajo /api.
export const API_ORIGIN = API_BASE_URL.replace(/\/api\/?$/, '');

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