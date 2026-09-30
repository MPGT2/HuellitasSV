# HuellitasSV — App móvil

App móvil (Expo / React Native) de la plataforma de adopción y rescate de mascotas **HuellitasSV** (El Salvador).

Consume la API .NET 10 que vive en [`../HuellitasSV.API`](../HuellitasSV.API).

---

## Stack

| | |
|---|---|
| Expo SDK | 57 |
| React Native | 0.86 |
| React | 19.2 |
| Navegación | React Navigation 7 (`native-stack`) |
| Estado de sesión | React Context (`AuthContext`) + `AsyncStorage` |
| HTTP | `fetch` nativo, sin librerías |
| Lenguaje | JavaScript (sin TypeScript, por decisión del proyecto) |

---

## Puesta en marcha

### 1. Levantar la API

La app necesita la API corriendo **en la red local**, no solo en `localhost`: el teléfono no puede alcanzar `localhost` de tu PC.

Desde la raíz del repositorio:

```bash
cd HuellitasSV.API
dotnet run --launch-profile lan
```

El perfil `lan` escucha en `0.0.0.0:5299`. El perfil `http` normal escucha solo en `localhost:5299` y **no sirve para el móvil**.

> Si el Windows Firewall pregunta, permite el puerto 5299 en red privada.

### 2. Levantar la app

```bash
cd mobile
npm install
npx expo start
```

Escanea el QR con Expo Go. La URL de la API se detecta sola (ver más abajo), no hay que editar ningún archivo.

---

## Configuración de la URL del backend

`src/config/env.js` resuelve la URL en este orden:

1. **`EXPO_PUBLIC_API_URL`** — si está definido en `mobile/.env`, gana. Formato: `http://192.168.1.68:5299/api`
2. **Auto-detección** — deriva la IP del propio servidor de Metro desde `Constants.expoConfig.hostUri`. Como el teléfono ya conoce esa IP (escaneó el QR), resuelve solo y sobrevive a que el DHCP te cambie la IP.
3. **Fallback** — `http://10.0.2.2:5299/api`, para emulador de Android.

Para fijarla a mano, crea `mobile/.env`:

```
EXPO_PUBLIC_API_URL=http://192.168.1.68:5299/api
```

`.env` está en `.gitignore`.

---

## Estructura

```
mobile/
├── App.js                  # Providers + NavigationContainer
├── index.js                # punto de entrada de Expo
├── app.json                # configuración de Expo
├── assets/                 # iconos y splash
├── docs/                   # notas de arquitectura del móvil
└── src/
    ├── components/         # FormField, PrimaryButton, RoleCard, RoleTabs
    ├── context/            # AuthContext (sesión)
    ├── navigation/         # AppNavigator (stacks por rol)
    ├── screens/            # 9 pantallas
    ├── services/           # api.js: cliente HTTP + endpoints
    ├── theme/              # colors.js
    └── config/             # env.js: URL del backend
```

### Pantallas

| Pantalla | Rol | Estado |
|---|---|---|
| `RoleSelectionScreen` | ambos | funcionando |
| `LoginScreen` | ambos | funcionando |
| `RegisterScreen` | ambos | funcionando |
| `CatalogScreen` (home) | Usuario | funcionando |
| `PetDetailScreen` | ambos | funciona |
| `AdoptionRequestScreen` | Usuario | funciona |
| `MyRequestsScreen` | Usuario | parcial: falta pantalla de detalle |
| `ProfileScreen` | ambos | parcial: no se autocompleta |
| `RefugioDashboardScreen` | Refugio | parcial: solo lista y aprueba solicitudes |

Módulos con API lista pero **sin interfaz**: reportes de animales, necesidades de donación, anuncios y notificaciones.

---

## Autenticación

El login es **unificado**: un solo endpoint `POST /api/Auth/login` para los tres roles. El backend determina el rol desde la cuenta y lo devuelve en la respuesta, así que la app no elige rol — solo guarda la sesión y `AppNavigator` rutea al stack correspondiente.

Flujo: `LoginScreen` → `AuthContext.login()` → `services/api.js` → guarda token y usuario en `AsyncStorage` → `AppNavigator` monta `UsuarioStack`, `RefugioStack` o `PublicStack`.

---

## Pendientes conocidos

- `MyRequestsScreen` navega a `SolicitudDetail`, una pantalla que no existe.
- `ProfileScreen` no se autocompleta al abrir; falta llamar a `getUsuarioPerfil` / `getRefugioPerfil`.
- Los filtros del catálogo solo se aplican al hacer pull-to-refresh (no hay botón "Aplicar").
- El formulario de registro de refugio pide "nombre completo" y lo descarta: el backend solo recibe `nombreOrganizacion`.
- `docs/` tiene material desactualizado respecto al código actual.
