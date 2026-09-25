# HuellitasSV Backend

API REST para la plataforma de adopción de mascotas **HuellitasSV**. Desarrollada con **ASP.NET Core (.NET 10)**, controladores tradicionales `[ApiController]`, **Entity Framework Core** y **SQL Server**.

## Descripción

El backend expone los servicios de acceso de usuarios (registro e inicio de sesión), catálogo público de mascotas, filtrado por atributos y ubicación, gestión de mascotas por parte de los refugios, y el ciclo completo de registro y aprobación de refugios gestionado por el administrador. Las contraseñas se almacenan con hash PBKDF2.

### Historias de usuario cubiertas

| HU | Historia | Endpoints principales |
|----|----------|----------------------|
| HU-1 | Gestión de acceso de usuarios (registro e inicio de sesión) | `POST /api/Usuarios/registro`, `POST /api/Usuarios/login` |
| HU-2 | Registro de refugio | `POST /api/Refugios/registro`, `POST /api/Refugios/login` |
| HU-4 | Ver mascotas por especie | `GET /api/Mascotas/especie/{especie}` |
| HU-5 | Filtrar por atributos | `GET /api/Mascotas/atributos` |
| HU-6 | Filtrar por ubicación | `GET /api/Mascotas/ubicacion` |
| HU-9 | Gestión de mascotas por el refugio | `GET/POST/PUT/DELETE /api/Mascotas` |
| HU-24 | Aprobación y control de refugios | `GET /api/Refugios/pendientes`, `PUT /api/Refugios/{id}/estado` |

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local, Docker o remoto)
- (Opcional) Herramientas de EF Core: `dotnet tool install --global dotnet-ef`

## Instalación

```bash
git clone <url-del-repositorio>
cd HuellitasSV_Backend/HuellitasSV.API
dotnet restore
```

## Configuración

La cadena de conexión se define en `HuellitasSV.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1434;Database=HuellitasSV;User Id=sa;Password=Huellitas2026!;TrustServerCertificate=True;"
  }
}
```

Ajusta `Server`, `Database`, `User Id` y `Password` según tu entorno. La base de datos **no se crea manualmente**: al iniciar la API se aplican automáticamente todas las migraciones (tablas, índices y datos semilla).

## Ejecución

```bash
cd HuellitasSV.API
dotnet run
```

- URL base: `http://localhost:5299`
- Documentación interactiva (Swagger): `http://localhost:5299/swagger`

Alternativamente, abre `HuellitasSV.API.http` en VS Code (extensión REST Client) para ejecutar las peticiones de ejemplo de cada historia de usuario.

## Endpoints

### Mascotas (`/api/Mascotas`)

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/` | Catálogo público de mascotas disponibles | 200 |
| GET | `/{id}` | Detalle de una mascota | 200, 404 |
| GET | `/especie/{especie}` | Disponibles por especie (perro, gato, otro) | 200 |
| GET | `/atributos` | Filtros combinables: `especie`, `tamano`, `edadMinMeses`, `edadMaxMeses`, `estadoSalud` | 200 |
| GET | `/ubicacion` | Filtros: `departamento`, `municipio` (municipio exige departamento) | 200 |
| GET | `/refugio/{idRefugio}` | Disponibles de un refugio aprobado | 200 |
| POST | `/` | Registra una mascota en estado "disponible" (JSON) | 201, 400 |
| PUT | `/{id}` | Actualización parcial y transiciones de estado | 200, 400, 404 |
| DELETE | `/{id}` | Elimina una mascota | 200, 404 |

### Refugios (`/api/Refugios`)

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/pendientes` | Refugios pendientes de aprobación (panel admin) | 200 |
| GET | `/{id}/mascotas` | Mascotas del refugio (panel); filtro opcional `?estado=` | 200, 404 |
| POST | `/registro` | Registra refugio: valida correo y nombre únicos, crea cuenta con rol "Refugio" y estado "pendiente" | 201, 400, 409 |
| POST | `/login` | Autenticación por correo y contraseña; solo refugios aprobados | 200, 401, 403 |
| PUT | `/{id}/estado` | Aprueba o rechaza un refugio; sincroniza el estado de su cuenta | 200, 400, 404 |

### Usuarios (`/api/Usuarios`)

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| POST | `/registro` | Registra un usuario cliente: valida nombre, correo y contraseña; crea la cuenta con rol "Usuario" y estado "activo" | 201, 400, 409 |
| POST | `/login` | Autenticación por correo y contraseña; rechaza cuentas inactivas o bloqueadas informando el motivo | 200, 401, 403 |

## Modelo de datos

| Tabla | Descripción |
|-------|-------------|
| `mascota` | Mascotas con especie, tamaño, edad, estado de salud, estado (`disponible`, `adoptada`, `fallecida`, `en_tratamiento`, `reservada`) e imagen (URL o BLOB). |
| `refugio` | Refugios con ubicación (departamento/municipio), contacto y `estado_aprobacion` (`pendiente`, `aprobado`, `rechazado`). |
| `cuenta` | Cuentas de acceso con correo único, contraseña con hash PBKDF2, rol (`Refugio`, `Usuario`, `Admin`), estado (por rol: `pendiente`/`aprobado`/`rechazado` o `activo`/`inactivo`/`bloqueado`). |
| `usuario` | Perfil de los usuarios clientes (nombre) vinculado a su cuenta. |

## Datos semilla

La base de datos se pobla automáticamente (migraciones `SeedData`, `DatosPrueba` y `AgregarTablaUsuario`) con 8 refugios, 16 mascotas y 3 usuarios que cubren todos los casos de los endpoints. Las cuentas de refugio usan la contraseña `Refugio2026!` y las de usuario `Usuario2026!`.

| Cuenta | Correo | Estado |
|--------|--------|--------|
| Refugio Huellitas San Salvador | `contacto@huellitassv.org` | aprobado |
| Protección Animal Santa Tecla | `adopciones@proteccionsv.org` | aprobado |
| Albergue Canino San Miguel | `info@alberguesm.org` | pendiente |
| Refugio Los Amigos (San Salvador) | `losamigos.refugio@correo.com` | aprobado |
| Hogar Animal Santa Ana (Santa Ana) | `hogarsantaana@correo.com` | aprobado |
| Vida Animal Sonsonate (Sonsonate) | `vidasonsonate@correo.com` | pendiente |
| Patitas de Usulután (Usulután) | `patitasusulutan@correo.com` | pendiente |
| Ayuda Animal Chalatenango (Chalatenango) | `ayudaanimalchalate@correo.com` | rechazado |
| María López (usuario) | `marialopez@correo.com` | activo |
| Carlos Pérez (usuario) | `carlosperez@correo.com` | inactivo |
| Ana Gómez (usuario) | `anagomez@correo.com` | bloqueado |

### Guía rápida de pruebas por endpoint

| Endpoint | Qué probar | Resultado esperado |
|----------|-----------|--------------------|
| `GET /api/Refugios/pendientes` | Listar pendientes | Refugios 3, 9003 y 9004 |
| `POST /api/Refugios/registro` | Registrar refugio nuevo | 201 y estado "pendiente" |
| `POST /api/Refugios/login` | Cuenta aprobada / pendiente / rechazada | 200 / 403 / 403 |
| `PUT /api/Refugios/9004/estado` | Aprobar a "Patitas de Usulután" | 200 y luego su login da 200 |
| `POST /api/Usuarios/registro` | Registrar usuario nuevo | 201, rol "Usuario", estado "activo" |
| `POST /api/Usuarios/login` | Cuenta activa / credenciales erróneas / inactiva / bloqueada | 200 / 401 genérico / 403 con motivo / 403 con motivo |
| `GET /api/Mascotas` | Catálogo disponible | 10 mascotas disponibles |
| `GET /api/Mascotas/especie/gato` | Especie | 5 gatos disponibles |
| `GET /api/Mascotas/atributos?tamano=grande&estadoSalud=sano` | Filtros combinables | Rocky II y Duke |
| `GET /api/Mascotas/atributos?edadMinMeses=30&edadMaxMeses=60` | Rango de edad | Rex y Rocky II |
| `GET /api/Mascotas/ubicacion?departamento=Santa%20Ana` | Ubicación | Duke |
| `GET /api/Mascotas/refugio/9001` | Mascotas de refugio aprobado | Nina y Simón |
| `GET /api/Refugios/1/mascotas` | Panel del refugio (todos los estados) | Firulais, Michi, Rex, Rocky II, Mia y Bobby |
| `POST /api/Mascotas` | Registrar mascota en refugio 9001 | 201 en estado "disponible" |
| `PUT /api/Mascotas/9002/estado` → `{"estado":"adoptada"}` | Transición de estado | 200 |
| `DELETE /api/Mascotas/9002` | Eliminar mascota | 200 |

## Estructura del proyecto

```
HuellitasSV.API/
├── Controllers/        # MascotasController, RefugiosController y UsuariosController (API REST)
├── Data/               # ApplicationDbContext (EF Core)
├── DTOs/               # DTOs de entrada de mascotas
├── Models/             # Entidades: Mascota, Refugio, Cuenta, Usuario
├── Migrations/         # Migraciones de EF Core (se aplican al iniciar)
├── Swagger/            # Filtro de ejemplos para los DTOs
├── Program.cs          # Configuración del host: DI, CORS, Swagger, migraciones
└── appsettings.json    # Cadena de conexión y configuración
```

## Convenciones del código

- Controladores tradicionales `[ApiController]` con orden jerárquico estricto: constructor, GETs, POSTs, PUTs, DELETEs.
- Comentarios de documentación XML (`/// <summary>`) en todos los métodos y propiedades públicas.
- Respuestas en formato JSON mediante `ActionResult`; errores uniformes con la forma `{ "error": "..." }` o `{ "errores": [ ... ] }`.
- Valores de dominio normalizados en minúsculas (estados, especies, tamaños).
- Consultas de solo lectura con `AsNoTracking()` para mejor rendimiento.
- Contraseñas almacenadas con hash PBKDF2 (`PasswordHasher` de ASP.NET Core).