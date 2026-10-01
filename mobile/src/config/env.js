import Constants from 'expo-constants';
import { Platform } from 'react-native';

const API_PORT = 5299;

function fromEnv() {
  const raw = process.env.EXPO_PUBLIC_API_URL;
  if (!raw) return null;
  return raw.trim().replace(/\/+$/, '');
}

function fromHostUri() {
  // Metro publica hostUri como "<ip>:<puerto>" (p. ej. "192.168.1.68:8081"),
  // pero Expo Go lo entrega con esquema "exp://". El telefono ya conoce esa IP
  // porque escaneo el QR, asi que es la forma mas fiable de deducir la IP de la
  // PC: sobrevive a que el DHCP la cambie.
  const hostUri = Constants.expoConfig?.hostUri;
  if (!hostUri) return null;

  // quita cualquier esquema (exp://, http://, ...), la ruta y el puerto.
  // El puerto solo se quita si es numerico, para no romper un IPv6 como [::1].
  const host = hostUri
    .replace(/^[a-zA-Z][a-zA-Z0-9+.-]*:\/\//, '')
    .split('/')[0]
    .replace(/:\d+$/, '');

  return host ? `http://${host}:${API_PORT}/api` : null;
}

function fromPlatform() {
  // Retornar directamente la URL de Ngrok para que funcione en el APK del teléfono físico
  return 'https://herself-crystal-overthrow.ngrok-free.dev/api';
}

const override = fromEnv();
const detected = override ?? fromHostUri();

export const API_BASE_URL = detected ?? fromPlatform();

// De donde salio la URL, util para diagnosticar fallos de conexion.
export const API_RESOLUTION = override ? 'env' : detected ? 'auto' : 'fallback';

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
