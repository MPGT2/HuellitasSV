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
  // 10.0.2.2 es el alias que usa el emulador de Android para su host.
  if (Platform.OS === 'android') return `http://10.0.2.2:${API_PORT}/api`;
  // El simulador de iOS comparte la red del host, asi que localhost funciona.
  return `http://localhost:${API_PORT}/api`;
}

const override = fromEnv();
const detected = override ?? fromHostUri();

export const API_BASE_URL = detected ?? fromPlatform();

// De donde salio la URL, util para diagnosticar fallos de conexion.
export const API_RESOLUTION = override ? 'env' : detected ? 'auto' : 'fallback';
