import Constants from 'expo-constants';
import { Platform } from 'react-native';

// --- LÓGICA INTELIGENTE: 3 MODOS DE CONEXIÓN ---

// Modo 1: Variable de entorno (para build de APK/producción)
//   Define: EXPO_PUBLIC_API_URL=https://tu-url.com  al hacer expo start
//   O en Windows: set EXPO_PUBLIC_API_URL=https://tu-url.com
//   O en Mac: export EXPO_PUBLIC_API_URL=https://tu-url.com
function fromEnv() {
  const raw = process.env.EXPO_PUBLIC_API_URL;
  if (!raw) return null;
  return raw.trim().replace(/\/+$/, '');
}

// Modo 2: IP automática desde QR (Expo Go - desarrollo)
//   Escanea el QR con Expo Go y detecta la IP de tu PC automáticamente
function fromHostUri() {
  const hostUri = Constants.expoConfig?.hostUri;
  if (!hostUri) return null;

  const host = hostUri
    .replace(/^[a-zA-Z][a-zA-Z0-9+.-]*:\/\//, '')
    .split('/')[0]
    .replace(/:\d+$/, '');

  return host ? `http://${host}:5299/api` : null;
}

// Modo 3: URL fija (MonsterASP o tu dominio)
//   Pega aquí tu URL pública si la conoces
const FIXED_URL = 'https://tu-api-monsterasp.monsterasp.com';

// Prioridad: 1) Variable de entorno > 2) QR Expo Go > 3) URL fija
const override = fromEnv();
const detected = override ?? fromHostUri();
const apiBaseUrl = detected ?? FIXED_URL;

export const API_BASE_URL = apiBaseUrl;

// Historial: 'env' si pusiste tu propia URL, 'auto' si es desde QR, 'fixed' si usa la default
export const API_RESOLUTION = override ? 'env' : fromHostUri() ? 'auto' : 'fixed';

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