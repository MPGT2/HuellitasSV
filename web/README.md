# HuellitasSV – Portal web

React + Vite. Consume la misma API que la app móvil (`https://huellitassv.runasp.net`).

## Puesta en marcha

```bash
cd web
npm install
npm run dev
```

El portal queda en `http://localhost:5173`.

## Configuración

La URL de la API está en `.env`:

```
VITE_API_URL=https://huellitassv.runasp.net/api
```

Si cambias el `.env`, reinicia `npm run dev`.

## Qué puede hacer cada rol

El login es único (`POST /api/Auth/login`): la API devuelve el rol y el portal muestra lo que corresponde.

| Rol | Pantallas |
|---|---|
| Sin sesión | Catálogo con filtros, detalle de mascota, login, registro |
| Usuario | Solicitar adopción, mis solicitudes |
| Refugio | Panel: mis mascotas (registrar, editar, eliminar), solicitudes (aprobar/rechazar), rescates |
| Admin | Panel: refugios pendientes (aprobar/rechazar), anuncios (aprobar, confirmar pago, rechazar) |

## Estructura

- `src/services/api.js` – todas las llamadas a la API (mismos endpoints que `mobile/src/services/api.js`).
- `src/config/` – URL de la API, imágenes y opciones (especies, tamaños, departamentos).
- `src/context/` – sesión (token y rol).
- `src/hooks/useDatos.js` – carga de datos con estados de carga y error.
- `src/pages/` – una pantalla por archivo.
- `src/components/` – Layout, rutas protegidas, tarjeta de mascota, etc.

## CORS

La API debe permitir el origen del portal (`http://localhost:5173` en local
y el dominio donde se publique).
