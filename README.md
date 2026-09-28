# HuellitasSV Backend

API REST para la plataforma de adopción y rescate de mascotas **HuellitasSV** (El Salvador).

Desarrollada con **ASP.NET Core (.NET 10)**, controladores tradicionales `[ApiController]`, **Entity Framework Core 10** sobre **SQL Server** y documentación interactiva con **Swagger**.

---

## Índice

1. [Descripción](#descripción)
2. [Tecnologías, frameworks y librerías](#tecnologías-frameworks-y-librerías)
3. [Arquitectura](#arquitectura)
4. [Seguridad](#seguridad)
5. [Requisitos previos](#requisitos-previos)
6. [Instalación](#instalación)
7. [Configuración](#configuración)
8. [Ejecución](#ejecución)
9. [Correr en GitHub Codespaces](#correr-en-github-codespaces)
10. [Base de datos y migraciones](#base-de-datos-y-migraciones)
11. [Historias de usuario cubiertas](#historias-de-usuario-cubiertas)
12. [Endpoints](#endpoints)
13. [Modelo de datos](#modelo-de-datos)
14. [Datos semilla](#datos-semilla)
15. [Estructura del proyecto](#estructura-del-proyecto)
16. [Convenciones del código](#convenciones-del-código)
17. [Flujo de trabajo Git](#flujo-de-trabajo-git)

---

## Descripción

El backend expone los servicios de la plataforma:

- **Acceso**: registro e inicio de sesión de usuarios clientes y refugios, con ciclo de aprobación gestionado por el administrador.
- **Catálogo**: mascotas disponibles con filtrado por especie, atributos (tamaño, edad, salud) y ubicación (departamento/municipio).
- **Gestión de refugios**: administración del catálogo propio de cada refugio.
- **Adopciones**: envío de solicitudes de adopción por los usuarios y decisión (aprobar/rechazar) por parte del refugio, con notificaciones automáticas.
- **Donaciones**: publicación de necesidades urgentes de insumos por los refugios y registro de aportes de la comunidad con cobertura automática.
- **Reportes callejeros**: un usuario reporta un animal en situación de calle con ubicación; los refugios en un radio de 5 km reciben la alerta y los atienden desde su panel.
- **Anuncios**: el administrador gestiona y cobra espacios publicitarios a tiendas (aprobación, pago y expiración automática).

## Tecnologías, frameworks y librerías

| Categoría | Tecnología | Versión | Uso en el proyecto |
|-----------|------------|---------|--------------------|
| Runtime / SDK | [.NET](https://dotnet.microsoft.com) | 10.0 (`net10.0`) | Plataforma base del proyecto |
| Framework web | [ASP.NET Core](https://learn.microsoft.com/aspnet/core) (SDK Web) | 10.0 | API REST con controladores `[ApiController]` |
| ORM | [Entity Framework Core](https://learn.microsoft.com/ef/core) | 10.0.12 | Acceso a datos; historial Code First en `Migrations/` (no se aplica al arrancar en cloud) |
| Proveedor BD | `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.12 | Conexión a SQL Server (`Microsoft.Data.SqlClient`) |
| Herramientas EF | `Microsoft.EntityFrameworkCore.Design` / `.Tools` | 10.0.12 | Scaffolding de migraciones con `dotnet ef` (CLI) |
| Documentación API | [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) | 10.2.3 | Swagger / Swagger UI en `/swagger` con ejemplos de DTOs |
| Documentación XML | `GenerateDocumentationFile` + `IncludeXmlComments` | — | Los comentarios XML (`/// <summary>`) alimentan la documentación de Swagger |
| Seguridad | `PasswordHasher<T>` (ASP.NET Core Identity) | incluido en el framework | Hash PBKDF2 de contraseñas |
| Seguridad | `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.12 | Autenticación por token JWT (`[Authorize]`, esquema Bearer) |
| Tokens | `System.IdentityModel.Tokens.Jwt` / `Microsoft.IdentityModel.Tokens` | 8.19.2 | Emisión y firma de tokens JWT (HMAC-SHA256) con claims de rol y perfil |
| Serialización | `System.Text.Json` (`JsonStringEnumConverter`) | incluido en el framework | Los enums (`SolicitudEstado`, `ReporteEstado`) se serializan como texto |
| CORS | Middleware CORS de ASP.NET Core | incluido en el framework | Política `PermitirFrontend` (cualquier origen/método/header) |
| Validación | Data Annotations (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`, …) | incluido en el framework | Validación de entrada automática en los DTOs |
| Configuración | `IConfiguration` + User Secrets / `appsettings.Local.json` | incluido en el framework | Cadena MonsterASP fuera del repo |
| BD | [SQL Server](https://www.microsoft.com/sql-server) en **MonsterASP** | — | Persistencia en la nube (esquema y datos ya desplegados) |
| Cliente REST | `.http` (VS Code REST Client) / Swagger UI | — | Pruebas manuales de los endpoints |

### Paquetes NuGet (`HuellitasSV.API.csproj`)

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.12" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.12" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.12" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.12" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.2.3" />
```

## Arquitectura

Arquitectura en capas monolítica, patrón **API REST + ORM Code First**:

```
HTTP → UseAuthentication (JWT) → UseAuthorization ([Authorize]) → Controllers → EF Core (DbSet/LINQ) → SQL Server (MonsterASP)
                        ↑
        Models + DTOs (validación con Data Annotations)
```

- **Controllers**: controlan el flujo HTTP, validan reglas de negocio y responden con `ActionResult` y códigos HTTP explícitos (200/201/400/401/403/404/409).
- **Models**: entidades POCO mapeadas a tablas con atributos `[Table]`/`[Column]` (nombres snake_case en la BD).
- **Data**: `ApplicationDbContext` configura claves, índices, precisiones y relaciones (`OnModelCreating`) además de los datos semilla.
- **DTOs**: objetos de entrada por request, desacoplando el contrato HTTP del modelo de persistencia.
- **Security**: `JwtTokenService` emite los tokens JWT firmados que usan los endpoints protegidos.
- **Errores uniformes**: errores de validación con forma `{ "errores": [ ... ] }` (`InvalidModelStateResponseFactory`) y errores de negocio con `{ "error": "..." }`.

## Seguridad

La API implementa autenticación y autorización con **JWT (JSON Web Tokens)**:

1. **Emisión de tokens**: los logins (`POST /api/Auth/login`, `POST /api/Usuarios/login`, `POST /api/Refugios/login`) devuelven un JWT firmado con HMAC-SHA256 (`Security/JwtTokenService`), con vigencia configurable (`Jwt:ExpiryMinutes`, por defecto 8 horas).
2. **Claims incluidos en el token**:
   - `sub`: identificador de la cuenta (`id_cuenta`).
   - Rol: `Admin`, `Refugio` o `Usuario` (usado por `[Authorize(Roles = "...")]`).
   - `idRefugio`: identificador del refugio asociado (solo rol Refugio).
   - `idUsuario`: identificador del perfil (solo rol Usuario).
   - `given_name`: nombre visible.
3. **Autorización por rol** con `[Authorize(Roles = "...")]`: los endpoints de gestión exigen el rol correspondiente y toman los identificadores del **token**, nunca de parámetros enviados por el cliente (se ignoran `refugioId`/`idRefugio`/`idUsuario` del query o del body).
4. **Protección adicional**: los usuarios solo consultan/modifican recursos propios (solicitudes de adopción y notificaciones se validan contra el claim del token).

### Matriz de protección de endpoints

| Nivel | Endpoints |
|-------|-----------|
| Público (sin token) | Catálogo y filtros de mascotas, registro/login de usuarios y refugios, `POST /api/Auth/login`, listado de anuncios, listado de necesidades de donación, crear solicitud de anuncio |
| 🔒 Rol `Usuario` | Envío/consulta de solicitudes de adopción (HU-7), crear reportes callejeros (HU-14), aportar a una necesidad de donación |
| 🔒 Rol `Refugio` | CRUD de mascotas (HU-9), gestión de solicitudes (HU-8), panel de rescate y atención de reportes (HU-15), publicar necesidades de donación |
| 🔒 Rol `Admin` | Aprobar/rechazar refugios (HU-24), aprobar/confirmar pago/rechazar anuncios (HU-13) |
| 🔒 Cualquier rol autenticado | Notificaciones propias (consulta y marcar leída) |

### Cómo obtener un token

```http
POST /api/Auth/login
Content-Type: application/json

{ "correo": "marialopez@correo.com", "contrasena": "Usuario2026!" }
```

Respuesta:

```json
{
  "mensaje": "Autenticación exitosa.",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "idCuenta": 9006,
  "rol": "Usuario",
  "nombre": "María López",
  "idRefugio": null,
  "idUsuario": 9001
}
```

En Swagger usa el botón **Authorize** (parte superior derecha) y pega el token para probar los endpoints protegidos. En `HuellitasSV.API.http` hay ejemplos de cada login.

### Credenciales semilla por rol

| Rol | Correo | Contraseña |
|-----|--------|-------------|
| Admin | `admin@huellitassv.org` | `Admin2026!` |
| Refugio (aprobado) | `losamigos.refugio@correo.com` | `Refugio2026!` |
| Usuario (activo) | `marialopez@correo.com` | `Usuario2026!` |

> La clave de firma JWT se define en `appsettings.json` (`Jwt:Key`) como clave de desarrollo. En producción debe sobrescribirse con user secrets o variables de entorno y **nunca** versionarse.

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Acceso a la base de datos **SQL Server en MonsterASP** (cadena de conexión del panel del hosting)
- (Opcional) `dotnet tool install --global dotnet-ef` solo si vas a crear migraciones nuevas de esquema

## Instalación

```bash
git clone https://github.com/MPGT2/HuellitasSV.git
cd HuellitasSV/HuellitasSV.API
dotnet restore
```

## Configuración

La API **no** incluye la cadena de conexión en el repositorio. Cada miembro del equipo debe apuntar a la BD de **MonsterASP**.

### Opción A — User secrets (recomendada)

```bash
cd HuellitasSV.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=TU_SERVIDOR.monsterasp.net;Database=TU_BD;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;Encrypt=True;"
```

### Opción B — Archivo local (no se versiona)

```bash
cd HuellitasSV.API
cp appsettings.Local.json.example appsettings.Local.json
# Edita appsettings.Local.json con la cadena real de MonsterASP
```

`appsettings.Local.json` está en `.gitignore`.

### Opción C — Variable de entorno

```bash
# PowerShell
$env:ConnectionStrings__DefaultConnection = "Server=....monsterasp.net;Database=...;User Id=...;Password=...;TrustServerCertificate=True;Encrypt=True;"

# bash
export ConnectionStrings__DefaultConnection='Server=....monsterasp.net;...'
```

En `appsettings.json` la cadena va vacía a propósito. Si falta, la API **no arranca** y muestra un mensaje claro.

> La clave JWT de desarrollo está en `appsettings.json`. En producción sobrescríbela con user secrets (`Jwt:Key`) o `Jwt__Key`. **Nunca** subas passwords ni claves reales al repo.

## Ejecución

```bash
cd HuellitasSV.API
dotnet run
```

- URL base: `http://localhost:5299` (`Properties/launchSettings.json`; el perfil `https` también expone `https://localhost:7040`)
- Swagger (solo Development): `http://localhost:5299/swagger`
- Al iniciar, la API **verifica la conexión** a MonsterASP. **No aplica migraciones** por defecto (`Database:ApplyMigrationsOnStartup: false`): el esquema y los datos ya están en la nube.

Alternativamente, abre `HuellitasSV.API.http` (extensión REST Client) para probar endpoints.

### Ejemplo de resultado JSON (login)

```http
POST /api/Auth/login
Content-Type: application/json

{ "correo": "marialopez@correo.com", "contrasena": "Usuario2026!" }
```

```json
{
  "mensaje": "Autenticación exitosa.",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "idCuenta": 9006,
  "rol": "Usuario",
  "nombre": "María López",
  "idRefugio": null,
  "idUsuario": 9001
}
```

### Ejemplo de resultado JSON (catálogo)

```http
GET /api/Mascotas/especie/perro
Accept: application/json
```

```json
[
  {
    "idMascota": 1,
    "nombre": "Rocky",
    "especie": "perro",
    "tamano": "mediano",
    "edadMeses": 24,
    "estadoSalud": "sano",
    "estado": "disponible",
    "idRefugio": 1
  }
]
```

## Correr en GitHub Codespaces

El `.devcontainer/` incluye **.NET 10 SDK**. La BD es la de **MonsterASP** (no se levanta SQL local).

1. **Code → Codespaces → Create codespace** en `main` o `develop`.
2. Define el secreto/variable `MONSTERASP_CONNECTION_STRING` en el Codespace (o configúrala a mano con user secrets).
3. Arranca:

   ```bash
   cd HuellitasSV.API
   dotnet run
   ```

4. Puerto **5299** → Swagger en la pestaña Puertos.

## Base de datos y migraciones

Proyecto **EF Core Code First**. El esquema y los datos de prueba viven en **MonsterASP**. Las carpetas `Migrations/` se conservan como historial del modelo EF; **no hace falta** ejecutar `dotnet ef database update` ni migrar al arrancar para trabajar en el día a día.

| Escenario | Qué hacer |
|-----------|-----------|
| Equipo / entrega (BD cloud) | Solo configurar la cadena. `ApplyMigrationsOnStartup = false` |
| Cambió el modelo y hay que alterar la BD cloud | Crear migración con `dotnet ef migrations add ...` y aplicarla de forma controlada (manual o con flag) |
| Laboratorio local excepcional | `Database:ApplyMigrationsOnStartup = true` en secrets/local |

```bash
dotnet ef migrations list          # listar historial de esquema
dotnet ef migrations add <Nombre>  # solo si cambió el modelo
```

Para forzar migraciones al arrancar (no recomendado contra la BD compartida del equipo):

```bash
dotnet user-secrets set "Database:ApplyMigrationsOnStartup" "true"
```

> **Importante:** no actives migraciones automáticas contra la BD compartida de MonsterASP sin coordinar con el equipo.
## Historias de usuario cubiertas

| HU | Historia | Módulo / Controladores |
|----|----------|------------------------|
| HU-01 | Gestión de acceso de usuarios (registro e inicio de sesión) | `UsuariosController` |
| HU-02 | Registro de refugio (con ciclo de aprobación) | `RefugiosController` |
| HU-04 | Ver mascotas por especie | `MascotasController` |
| HU-05 | Filtrar mascotas por atributos | `MascotasController` |
| HU-06 | Filtrar mascotas por ubicación | `MascotasController` |
| HU-07 | Enviar solicitud de adopción | `SolicitudesAdopcionController` |
| HU-08 | Gestión de solicitudes de adopción por el refugio | `GestionSolicitudesController` |
| HU-09 | Gestión de mascotas por el refugio | `MascotasController` |
| HU-13 | Gestión y cobro de anuncios publicitarios (admin) | `AnunciosController` |
| HU-14 | Reportar animal en situación de calle | `ReportesAnimalesController` |
| HU-15 | Panel de rescate: refugio recibe y atiende reportes | `ReportesRescateController` |
| HU-24 | Aprobación y control de refugios (admin) | `RefugiosController` |
| HU-09 | Publicación de necesidades de donación y aportes de la comunidad (extensión de la gestión del refugio) | `NecesidadesDonacionController` |
| HU-07/08/14/15 | Consulta y marcado de notificaciones (parte de los flujos de adopciones y reportes) | `NotificacionesController` |
## Endpoints

Los endpoints marcados con 🔒 exigen token JWT (`Authorization: Bearer <token>`) y rol. Ver [Seguridad](#seguridad).

### Autenticación (`/api/Auth`) — seguridad

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| POST | `/login` | Login único con JWT para cualquier rol (Admin, Refugio, Usuario); devuelve el token con los claims del perfil | 200, 401, 403 |

### Usuarios (`/api/Usuarios`) — HU-01

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| POST | `/registro` | Registra un usuario cliente: crea cuenta con rol "Usuario", estado "activo" y su perfil | 201, 400, 409 |
| POST | `/login` | Autenticación por correo y contraseña; devuelve JWT; rechaza cuentas inactivas o bloqueadas informando el motivo | 200, 401, 403 |

### Refugios (`/api/Refugios`) — HU-02 / HU-24

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET 🔒 | `/pendientes` | Refugios pendientes de aprobación (solo Admin) | 200 |
| GET | `/{id}/mascotas` | Mascotas del refugio (panel); filtro opcional `?estado=` | 200, 404 |
| POST | `/registro` | Registra refugio: valida correo y nombre únicos, crea cuenta con rol "Refugio" y estado "pendiente" | 201, 400, 409 |
| POST | `/login` | Autenticación por correo y contraseña; solo refugios aprobados; devuelve JWT | 200, 401, 403 |
| PUT 🔒 | `/{id}/estado` | Aprueba o rechaza un refugio (solo Admin); sincroniza el estado de su cuenta | 200, 400, 404 |

### Mascotas (`/api/Mascotas`) — HU-04 / HU-05 / HU-06 / HU-09

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/` | Catálogo público de mascotas disponibles | 200 |
| GET | `/{id}` | Detalle de una mascota | 200, 404 |
| GET | `/especie/{especie}` | Disponibles por especie (perro, gato, otro) | 200 |
| GET | `/atributos` | Filtros combinables: `especie`, `tamano`, `edadMinMeses`, `edadMaxMeses`, `estadoSalud` | 200 |
| GET | `/ubicacion` | Filtros: `departamento`, `municipio` (municipio exige departamento) | 200 |
| GET | `/refugio/{idRefugio}` | Disponibles de un refugio aprobado | 200 |
| POST 🔒 | `/` | Registra una mascota en estado "disponible" (solo Refugio; refugio del token) | 201, 400 |
| PUT 🔒 | `/{id}` | Actualización parcial y transiciones de estado (solo el refugio dueño) | 200, 400, 403, 404 |
| DELETE 🔒 | `/{id}` | Elimina una mascota (solo el refugio dueño) | 200, 403, 404 |

### Solicitudes de adopción — HU-07 / HU-08

Envío por el usuario y decisión por el refugio.

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| POST 🔒 | `/api/SolicitudesAdopcion` | Envía una solicitud de adopción (usuario del token) | 201, 400, 404, 409 |
| GET 🔒 | `/api/SolicitudesAdopcion/{id}` | Detalle de la solicitud (solo si es propia) | 200, 403, 404 |
| GET 🔒 | `/api/GestionSolicitudes?estado=` | Solicitudes recibidas por el refugio del token (con filtros) | 200 |
| GET 🔒 | `/api/GestionSolicitudes/{id}` | Detalle para el refugio | 200, 404 |
| PUT 🔒 | `/api/GestionSolicitudes/{id}/aprobar` | Aprueba la solicitud (notifica al usuario; una mascota solo puede tener una aprobada) | 200, 400, 403, 404, 409 |
| PUT 🔒 | `/api/GestionSolicitudes/{id}/rechazar` | Rechaza con comentario (notifica al usuario) | 200, 400, 403, 404 |
### Anuncios (`/api/Anuncios`) — HU-13

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/?estado=activo` | Anuncios visibles (activos, con pago confirmado y vigentes); `?estado=pendiente|vencido|todas` para administración | 200 |
| POST | `/` | Registra solicitud de espacio publicitario de una tienda | 201, 400 |
| POST 🔒 | `/{id}/aprobar` | El admin aprueba el anuncio y define fechas (solo Admin) | 200, 400, 404, 409 |
| POST 🔒 | `/{id}/confirmar-pago` | Confirma el pago del espacio (solo Admin) | 200, 400, 404, 409 |
| POST 🔒 | `/{id}/rechazar` | Rechaza el anuncio (solo Admin) | 200, 400, 404, 409 |

La expiración a "vencido" se recalcula automáticamente en cada consulta (sin scheduler en segundo plano).

### Necesidades de donación (`/api/NecesidadesDonacion`) — HU-09

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/` | Necesidades publicadas, con cobertura acumulada y porcentaje | 200 |
| POST 🔒 | `/` | El refugio publica una necesidad urgente de insumos (refugio del token) | 201, 400 |
| POST 🔒 | `/{id}/aportar` | La comunidad registra un aporte (solo Usuario); actualiza `CantidadCubierta` | 200, 400, 404 |

### Notificaciones (`/api/Notificaciones`) — parte de HU-07/08/14/15

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET 🔒 | `/?leidas=` | Notificaciones del destinatario autenticado (del token); filtro por lectura | 200 |
| PUT 🔒 | `/{id}/leida` | Marca una notificación como leída (solo si es propia) | 200, 403, 404 |

### Reportes de animales callejeros — HU-14 / HU-15

Un reporte incluye ubicación (latitud/longitud); al crearlo se notifica automáticamente a los refugios aprobados dentro de un radio de **5 km** (fórmula de Haversine).

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| POST 🔒 | `/api/ReportesAnimales` | Crea un reporte y notifica a los refugios cercanos (usuario del token) | 201, 400 |
| GET 🔒 | `/api/ReportesAnimales/{id}` | Detalle del reporte | 200, 404 |
| GET 🔒 | `/api/ReportesRescate?estado=` | Panel del refugio: reportes cercanos con distancia (refugio del token) | 200 |
| GET 🔒 | `/api/ReportesRescate/{id}` | Detalle para el refugio | 200, 404 |
| PUT 🔒 | `/api/ReportesRescate/{id}/atendido` | El refugio marca el reporte como atendido (quien lo atiende primero) | 200, 400, 404, 409 |

## Modelo de datos

Esquema en MonsterASP (9 tablas; historial EF en `Migrations/`):

| Tabla | Descripción |
|-------|-------------|
| `cuenta` | Cuentas de acceso con correo único, contraseña con hash PBKDF2, rol (`Refugio`, `Usuario`, `Admin`) y estado (por rol: `pendiente`/`aprobado`/`rechazado` o `activo`/`inactivo`/`bloqueado`). |
| `usuario` | Perfil de los usuarios clientes (nombre) vinculado a su cuenta (`id_cuenta`). |
| `refugio` | Refugios con ubicación (departamento/municipio), contacto, coordenadas opcionales y `estado_aprobacion` (`pendiente`, `aprobado`, `rechazado`). |
| `mascota` | Mascotas con especie, tamaño, edad, estado de salud, estado (`disponible`, `adoptada`, `fallecida`, `en_tratamiento`, `reservada`) e imagen (URL o BLOB). |
| `SolicitudesAdopcion` | Solicitudes de adopción con datos de contacto, estado (`Pendiente`, `Aprobada`, `Rechazada`) y comentario de decisión. FK a `mascota` y `usuario`. |
| `Notificaciones` | Mensajes a refugios o usuarios (creación/decisión de solicitudes, alertas de reportes). Estado `Leida`. |
| `ReportesAnimales` | Reportes de animales en calle con descripción, foto (URL), coordenadas, estado (`Pendiente`, `Atendido`) y refugio que atiende. |
| `NecesidadesDonacion` | Necesidades urgentes de insumos por refugio con `CantidadRequerida` y `CantidadCubierta` (`decimal(18,2)`). |
| `anuncio` | Espacios publicitarios: tienda, contacto, imagen, precio (`decimal(10,2)`), fechas de vigencia, `pago_confirmado` y estado (`pendiente`, `activo`, `rechazado`, `vencido`). |

Relaciones clave: `Mascota → Refugio` (borrado restrictivo), `SolicitudAdopcion → Mascota` (cascada) y `→ Usuario` (restrictivo), `Notificacion → Refugio/Usuario` (cascada), `ReporteAnimal → Usuario/Refugio` (restrictivo), `NecesidadDonacion → Refugio` (cascada).

## Datos semilla

Los datos de prueba están cargados en la BD de **MonsterASP**. Credenciales de referencia (contraseñas: Admin `Admin2026!`, Refugio `Refugio2026!`, Usuario `Usuario2026!`):

| Cuenta | Correo | Rol | Estado |
|--------|--------|-----|--------|
| Refugio Huellitas San Salvador | `contacto@huellitassv.org` | Refugio | aprobado |
| Protección Animal Santa Tecla | `adopciones@proteccionsv.org` | Refugio | aprobado |
| Albergue Canino San Miguel | `info@alberguesm.org` | Refugio | pendiente |
| Refugio Los Amigos (San Salvador) | `losamigos.refugio@correo.com` | Refugio | aprobado |
| Hogar Animal Santa Ana (Santa Ana) | `hogarsantaana@correo.com` | Refugio | aprobado |
| Vida Animal Sonsonate (Sonsonate) | `vidasonsonate@correo.com` | Refugio | pendiente |
| Patitas de Usulután (Usulután) | `patitasusulutan@correo.com` | Refugio | pendiente |
| Ayuda Animal Chalatenango (Chalatenango) | `ayudaanimalchalate@correo.com` | Refugio | rechazado |
| María López (usuario) | `marialopez@correo.com` | Usuario | activo |
| Carlos Pérez (usuario) | `carlosperez@correo.com` | Usuario | inactivo |
| Ana Gómez (usuario) | `anagomez@correo.com` | Usuario | bloqueado |
| Administrador | `admin@huellitassv.org` | Admin | aprobado |

También se siembran 16 mascotas distribuidas en los refugios 1, 2, 3, 9001 y 9002 (disponibles, reservadas, adoptadas, en tratamiento y fallecidas) para cubrir catálogo, filtros y paneles.

## Estructura del proyecto

```
HuellitasSV.API/
├── Controllers/           # Controladores REST (orden: GETs → POSTs → PUTs → DELETEs)
│   ├── AuthController.cs               # Login único con JWT
│   ├── MascotasController.cs          # Catálogo, filtros y gestión (HU-4/5/6/9)
│   ├── UsuariosController.cs          # Registro/login de usuarios (HU-1)
│   ├── RefugiosController.cs          # Registro/login y aprobación (HU-2/24)
│   ├── SolicitudesAdopcionController.cs  # Envío de solicitudes (HU-7)
│   ├── GestionSolicitudesController.cs   # Decisión del refugio (HU-8)
│   ├── AnunciosController.cs          # Espacios publicitarios (HU-13)
│   ├── NecesidadesDonacionController.cs  # Donaciones (HU-9)
│   ├── NotificacionesController.cs    # Notificaciones
│   ├── ReportesAnimalesController.cs  # Reportes callejeros (HU-14)
│   └── ReportesRescateController.cs   # Panel de rescate (HU-15)
├── Data/                  # ApplicationDbContext (EF Core)
├── DTOs/                  # Objetos de entrada (Data Annotations)
├── Models/                # Entidades EF Core (9 tablas)
├── Migrations/            # Historial de esquema (no se aplica al arrancar en cloud)
├── Security/              # JwtTokenService
├── Swagger/               # DtoExamplesSchemaFilter + Authorize
├── Program.cs             # Host: DI, CORS, Swagger, JWT; conexión cloud sin Migrate por defecto
├── appsettings.json       # Config base (cadena vacía; Jwt de desarrollo)
├── appsettings.Local.json.example  # Plantilla de secretos MonsterASP
└── HuellitasSV.API.http   # Peticiones de ejemplo
```

## Convenciones del código

- Controladores tradicionales `[ApiController]` con orden jerárquico estricto: constructor, GETs, POSTs, PUTs, DELETEs.
- Comentarios de documentación XML (`/// <summary>`) en todos los métodos y propiedades públicas, incluidos en Swagger.
- Respuestas en formato JSON mediante `ActionResult`; errores uniformes con la forma `{ "error": "..." }` o `{ "errores": [ ... ] }`.
- Valores de dominio normalizados en minúsculas (estados, especies, tamaños).
- Consultas de solo lectura con `AsNoTracking()` para mejor rendimiento; `FindAsync`/`FirstOrDefaultAsync` para acceso por clave.
- Claves explícitas con `HasKey` en `OnModelCreating` (el formato `IdXxx` no coincide con la convención de EF Core).
- Contraseñas almacenadas con hash PBKDF2 (`PasswordHasher` de ASP.NET Core Identity); nunca se devuelven en respuestas (`[JsonIgnore]`).
- **Seguridad**: todo endpoint de gestión lleva `[Authorize(Roles = "...")]` y toma los identificadores (idRefugio/idUsuario) del token JWT, nunca de parámetros del cliente; los recursos ajenos responden 403.
- Numeración de entidades: los objetos semilla usan IDs 1–3 (base) y 9001+ (prueba); los registros nuevos continúan la secuencia identity.

## Flujo de trabajo Git

- `feature/HU-XX` → desarrollo por historia de usuario.
- `develop` → rama de integración; se incorporan las features con merge `--no-ff`.
- `main` → versión estable; recibe `develop` cuando la integración compila y los endpoints están validados.
- Mensajes de commit con el formato `[HU-XX] Nombre: descripción`.