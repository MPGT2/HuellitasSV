# HuellitasSV Backend

API REST para la plataforma de adopción y rescate de mascotas **HuellitasSV** (El Salvador).

Desarrollada con **ASP.NET Core (.NET 10)**, controladores tradicionales `[ApiController]`, **Entity Framework Core 10** sobre **SQL Server** y documentación interactiva con **Swagger**.

---

## Índice

1. [Descripción](#descripción)
2. [Tecnologías, frameworks y librerías](#tecnologías-frameworks-y-librerías)
3. [Arquitectura](#arquitectura)
4. [Requisitos previos](#requisitos-previos)
5. [Instalación](#instalación)
6. [Configuración](#configuración)
7. [Ejecución](#ejecución)
8. [Base de datos y migraciones](#base-de-datos-y-migraciones)
9. [Historias de usuario cubiertas](#historias-de-usuario-cubiertas)
10. [Endpoints](#endpoints)
11. [Modelo de datos](#modelo-de-datos)
12. [Datos semilla](#datos-semilla)
13. [Estructura del proyecto](#estructura-del-proyecto)
14. [Convenciones del código](#convenciones-del-código)
15. [Flujo de trabajo Git](#flujo-de-trabajo-git)

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
| ORM | [Entity Framework Core](https://learn.microsoft.com/ef/core) | 10.0.12 | Acceso a datos, migraciones Code First y datos semilla (`HasData`) |
| Proveedor BD | `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.12 | Conexión a SQL Server (`Microsoft.Data.SqlClient`) |
| Herramientas EF | `Microsoft.EntityFrameworkCore.Design` / `.Tools` | 10.0.12 | Scaffolding de migraciones con `dotnet ef` (CLI) |
| Documentación API | [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) | 10.2.3 | Swagger / Swagger UI en `/swagger` con ejemplos de DTOs |
| Documentación XML | `GenerateDocumentationFile` + `IncludeXmlComments` | — | Los comentarios XML (`/// <summary>`) alimentan la documentación de Swagger |
| Seguridad | `PasswordHasher<T>` (ASP.NET Core Identity) | incluido en el framework | Hash PBKDF2 de contraseñas (sin JWT ni sesiones: validación por credenciales por request) |
| Serialización | `System.Text.Json` (`JsonStringEnumConverter`) | incluido en el framework | Los enums (`SolicitudEstado`, `ReporteEstado`) se serializan como texto |
| CORS | Middleware CORS de ASP.NET Core | incluido en el framework | Política `PermitirFrontend` (cualquier origen/método/header) |
| Validación | Data Annotations (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`, …) | incluido en el framework | Validación de entrada automática en los DTOs |
| Configuración | `IConfiguration` + User Secrets | incluido en el framework | Cadena de conexión en `appsettings.json` y/o secretos locales de desarrollo |
| BD | [SQL Server](https://www.microsoft.com/sql-server) | 2019+ / Express | Persistencia (local, Docker o remoto) |
| Cliente REST | `.http` (VS Code REST Client) / Swagger UI | — | Pruebas manuales de los endpoints |

### Paquetes NuGet (`HuellitasSV.API.csproj`)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.12" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.12" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.12" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.2.3" />
```

## Arquitectura

Arquitectura en capas monolítica, patrón **API REST + ORM Code First**:

```
HTTP → Controllers → EF Core (DbSet/LINQ) → SQL Server
                ↑
        Models + DTOs (validación con Data Annotations)
```

- **Controllers**: controlan el flujo HTTP, validan reglas de negocio y responden con `ActionResult` y códigos HTTP explícitos (200/201/400/401/403/404/409).
- **Models**: entidades POCO mapeadas a tablas con atributos `[Table]`/`[Column]` (nombres snake_case en la BD).
- **Data**: `ApplicationDbContext` configura claves, índices, precisiones y relaciones (`OnModelCreating`) además de los datos semilla.
- **DTOs**: objetos de entrada por request, desacoplando el contrato HTTP del modelo de persistencia.
- **Errores uniformes**: errores de validación con forma `{ "errores": [ ... ] }` (`InvalidModelStateResponseFactory`) y errores de negocio con `{ "error": "..." }`.

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local, Docker o remoto)
- (Opcional, para mantenimiento de migraciones) `dotnet tool install --global dotnet-ef`

## Instalación

```bash
git clone https://github.com/MPGT2/HuellitasSV.git
cd HuellitasSV/HuellitasSV_Backend/HuellitasSV.API
dotnet restore
```

## Configuración

La cadena de conexión se define en `HuellitasSV.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HuellitasSV;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Ajusta `Server`, `Database`, `User Id` y `Password` según tu entorno.

**Opción recomendada para desarrollo local** (sin tocar archivos versionados): user secrets.

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=HuellitasSV;Trusted_Connection=True;TrustServerCertificate=True;"
```

## Ejecución

```bash
cd HuellitasSV.API
dotnet run
```

- URL base: `http://localhost:5299` (definida en `Properties/launchSettings.json`; el perfil `https` expone además `https://localhost:7040`)
- Documentación interactiva (Swagger): `http://localhost:5299/swagger`
- Al iniciar, la API aplica automáticamente las migraciones pendientes (crea la BD, tablas, índices y datos semilla) — ver `Program.cs`.

Alternativamente, abre `HuellitasSV.API.http` en VS Code (extensión REST Client) para ejecutar las peticiones de ejemplo de cada historia de usuario.

## Base de datos y migraciones

Proyecto **EF Core Code First** con una única migración de arranque (`Migrations/20260925063826_Inicial`) que crea el esquema completo (9 tablas) y siembra los datos de prueba (`HasData`). Las contraseñas semilla se almacenan con hash PBKDF2 (Identity v3), nunca en texto plano.

Comandos útiles:

```bash
dotnet ef migrations list          # listar migraciones
dotnet ef migrations add <Nombre>  # crear nueva migración tras cambios de modelo
dotnet ef database update          # aplicar migraciones manualmente
dotnet ef database drop            # eliminar la BD local
```

> No es necesario ejecutarlos para correr la API: `Program.cs` llama a `db.Database.Migrate()` en el arranque.

## Historias de usuario cubiertas

| HU | Historia | Módulo / Controladores |
|----|----------|------------------------|
| HU-01 | Gestión de acceso de usuarios (registro e inicio de sesión) | `UsuariosController` |
| HU-02 | Registro de refugio (con ciclo de aprobación) | `RefugiosController` |
| HU-04 | Ver mascotas por especie | `MascotasController` |
| HU-05 | Filtrar mascotas por atributos | `MascotasController` |
| HU-06 | Filtrar mascotas por ubicación | `MascotasController` |
| HU-07 | Enviar solicitud de adopción | `SolicitudesAdopcionController` / `AdopcionSolicitudesController` |
| HU-08 | Gestión de solicitudes de adopción por el refugio | `GestionSolicitudesController` / `AdopcionGestionController` |
| HU-09 | Gestión de mascotas por el refugio | `MascotasController` |
| HU-13 | Gestión y cobro de anuncios publicitarios (admin) | `AnunciosController` |
| HU-14 | Reportar animal en situación de calle | `ReportesAnimalesController` / `ReporteCallejeroController` |
| HU-15 | Panel de rescate: refugio recibe y atiende reportes | `ReportesRescateController` / `ReporteCallejeroPanelController` |
| HU-24 | Aprobación y control de refugios (admin) | `RefugiosController` |
| — | Publicación de necesidades de donación y aportes de la comunidad | `NecesidadesDonacionController` |
| — | Consulta y marcado de notificaciones | `NotificacionesController` |

> **Nota de integración:** las HU-07/08 y HU-14/15 cuentan con **controladores paralelos** provenientes de ramas distintas (mismo comportamiento, rutas distintas). Ambos funcionan; la consolidación en un único controlador por HU está pendiente.

## Endpoints

### Usuarios (`/api/Usuarios`) — HU-01

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| POST | `/registro` | Registra un usuario cliente: crea cuenta con rol "Usuario", estado "activo" y su perfil | 201, 400, 409 |
| POST | `/login` | Autenticación por correo y contraseña; rechaza cuentas inactivas o bloqueadas informando el motivo | 200, 401, 403 |

### Refugios (`/api/Refugios`) — HU-02 / HU-24

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/pendientes` | Refugios pendientes de aprobación (panel admin) | 200 |
| GET | `/{id}/mascotas` | Mascotas del refugio (panel); filtro opcional `?estado=` | 200, 404 |
| POST | `/registro` | Registra refugio: valida correo y nombre únicos, crea cuenta con rol "Refugio" y estado "pendiente" | 201, 400, 409 |
| POST | `/login` | Autenticación por correo y contraseña; solo refugios aprobados | 200, 401, 403 |
| PUT | `/{id}/estado` | Aprueba o rechaza un refugio; sincroniza el estado de su cuenta | 200, 400, 404 |

### Mascotas (`/api/Mascotas`) — HU-04 / HU-05 / HU-06 / HU-09

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/` | Catálogo público de mascotas disponibles | 200 |
| GET | `/{id}` | Detalle de una mascota | 200, 404 |
| GET | `/especie/{especie}` | Disponibles por especie (perro, gato, otro) | 200 |
| GET | `/atributos` | Filtros combinables: `especie`, `tamano`, `edadMinMeses`, `edadMaxMeses`, `estadoSalud` | 200 |
| GET | `/ubicacion` | Filtros: `departamento`, `municipio` (municipio exige departamento) | 200 |
| GET | `/refugio/{idRefugio}` | Disponibles de un refugio aprobado | 200 |
| POST | `/` | Registra una mascota en estado "disponible" | 201, 400 |
| PUT | `/{id}` | Actualización parcial y transiciones de estado | 200, 400, 404 |
| DELETE | `/{id}` | Elimina una mascota | 200, 404 |

### Solicitudes de adopción — HU-07 / HU-08

Envío por el usuario y decisión por el refugio. Existen dos conjuntos paralelos (ver nota de integración):

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| POST | `/api/SolicitudesAdopcion` | Envía una solicitud de adopción | 201, 400, 404, 409 |
| GET | `/api/SolicitudesAdopcion/{id}` | Detalle de la solicitud | 200, 404 |
| GET | `/api/GestionSolicitudes?refugioId=&estado=` | Solicitudes recibidas por el refugio (con filtros) | 200 |
| GET | `/api/GestionSolicitudes/{id}?refugioId=` | Detalle para el refugio | 200, 404 |
| PUT | `/api/GestionSolicitudes/{id}/aprobar?refugioId=` | Aprueba la solicitud (notifica al usuario; una mascota solo puede tener una aprobada) | 200, 400, 404, 409 |
| PUT | `/api/GestionSolicitudes/{id}/rechazar?refugioId=` | Rechaza con comentario (notifica al usuario) | 200, 400, 404 |
| POST | `/api/Adopcionsolicitudes` | Envío de solicitud (variante) | 201 |
| GET | `/api/Adopcionsolicitudes/{id}` | Detalle (variante) | 200, 404 |
| GET | `/api/Adopciongestion?refugioId=` | Listado para decisión (variante) | 200 |
| PUT | `/api/Adopciongestion/{id}/aprobar` / `/rechazar` | Decisión (variante) | 200 |

### Anuncios (`/api/Anuncios`) — HU-13

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/?estado=activo` | Anuncios visibles (activos, con pago confirmado y vigentes); `?estado=pendiente|vencido|todas` para administración | 200 |
| POST | `/` | Registra solicitud de espacio publicitario de una tienda | 201, 400 |
| POST | `/{id}/aprobar` | El admin aprueba el anuncio y define fechas | 200, 400, 404, 409 |
| POST | `/{id}/confirmar-pago` | Confirma el pago del espacio | 200, 400, 404, 409 |
| POST | `/{id}/rechazar` | Rechaza el anuncio | 200, 400, 404, 409 |

La expiración a "vencido" se recalcula automáticamente en cada consulta (sin scheduler en segundo plano).

### Necesidades de donación (`/api/NecesidadesDonacion`)

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/` | Necesidades publicadas, con cobertura acumulada y porcentaje | 200 |
| POST | `/` | El refugio publica una necesidad urgente de insumos | 201, 400 |
| POST | `/{id}/aportar` | La comunidad registra un aporte; actualiza `CantidadCubierta` | 200, 400, 404 |

### Notificaciones (`/api/Notificaciones`)

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| GET | `/?refugioId=&usuarioId=&leidas=` | Notificaciones del destinatario con filtros | 200 |
| PUT | `/{id}/leida` | Marca una notificación como leída | 200, 404 |

### Reportes de animales callejeros — HU-14 / HU-15

Un reporte incluye ubicación (latitud/longitud); al crearlo se notifica automáticamente a los refugios aprobados dentro de un radio de **5 km** (fórmula de Haversine).

| Método | Ruta | Descripción | Códigos |
|--------|------|-------------|---------|
| POST | `/api/ReportesAnimales` | Crea un reporte y notifica a los refugios cercanos | 201, 400 |
| GET | `/api/ReportesAnimales/{id}` | Detalle del reporte | 200, 404 |
| GET | `/api/ReportesRescate?refugioId=&estado=` | Panel del refugio: reportes cercanos con distancia | 200 |
| GET | `/api/ReportesRescate/{id}` | Detalle para el refugio | 200, 404 |
| PUT | `/api/ReportesRescate/{id}/atendido` | El refugio marca el reporte como atendido (quien lo atiende primero) | 200, 400, 404, 409 |
| POST | `/api/Reportecallejero` | Creación de reporte (variante) | 201 |
| GET | `/api/Reportecallejero/{id}` | Detalle (variante) | 200 |
| GET | `/api/Reportecallejeropanel?refugioId=` | Panel del refugio (variante) | 200 |
| PUT | `/api/Reportecallejeropanel/{id}/atender` | Atención de reporte (variante) | 200 |

## Modelo de datos

Esquema creado por la migración `Inicial` (9 tablas):

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

La migración `Inicial` siembra: **11 cuentas**, **8 refugios**, **16 mascotas** y **3 usuarios**. Las cuentas de refugio usan la contraseña `Refugio2026!` y las de usuario `Usuario2026!`.

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

También se siembran 16 mascotas distribuidas en los refugios 1, 2, 3, 9001 y 9002 (disponibles, reservadas, adoptadas, en tratamiento y fallecidas) para cubrir catálogo, filtros y paneles.

## Estructura del proyecto

```
HuellitasSV.API/
├── Controllers/           # Controladores REST (orden jerárquico: GETs → POSTs → PUTs → DELETEs)
│   ├── MascotasController.cs          # Catálogo, filtros y gestión (HU-4/5/6/9)
│   ├── UsuariosController.cs          # Registro/login de usuarios (HU-1)
│   ├── RefugiosController.cs          # Registro/login y aprobación de refugios (HU-2/24)
│   ├── SolicitudesAdopcionController.cs / GestionSolicitudesController.cs   # Adopciones (HU-7/8)
│   ├── Adopcionsolicitudescontroller.cs / Adopciongestioncontroller.cs       # Adopciones (variantes)
│   ├── AnunciosController.cs          # Espacios publicitarios (HU-13)
│   ├── NecesidadesDonacionController.cs  # Donaciones
│   ├── NotificacionesController.cs    # Notificaciones
│   └── Reportes*Controller.cs         # Reportes callejeros y panel de rescate (HU-14/15)
├── Data/                  # ApplicationDbContext (EF Core): DbSets, índices, relaciones y seed
├── DTOs/                  # Objetos de entrada de cada endpoint (validación con Data Annotations)
├── Models/                # Entidades EF Core: Mascota, Refugio, Cuenta, Usuario, SolicitudAdopcion,
│                          # Notificacion, ReporteAnimal, NecesidadDonacion, Anuncio
├── Migrations/            # Migración Inicial (esquema completo + datos semilla); se aplica al iniciar
├── Swagger/               # DtoExamplesSchemaFilter (ejemplos válidos en Swagger)
├── Program.cs             # Host: DI, CORS, Swagger, errores uniformes y migraciones automáticas
├── appsettings.json       # Cadena de conexión y configuración
└── HuellitasSV.API.http   # Peticiones de ejemplo por HU (VS Code REST Client)
```

## Convenciones del código

- Controladores tradicionales `[ApiController]` con orden jerárquico estricto: constructor, GETs, POSTs, PUTs, DELETEs.
- Comentarios de documentación XML (`/// <summary>`) en todos los métodos y propiedades públicas, incluidos en Swagger.
- Respuestas en formato JSON mediante `ActionResult`; errores uniformes con la forma `{ "error": "..." }` o `{ "errores": [ ... ] }`.
- Valores de dominio normalizados en minúsculas (estados, especies, tamaños).
- Consultas de solo lectura con `AsNoTracking()` para mejor rendimiento; `FindAsync`/`FirstOrDefaultAsync` para acceso por clave.
- Claves explícitas con `HasKey` en `OnModelCreating` (el formato `IdXxx` no coincide con la convención de EF Core).
- Contraseñas almacenadas con hash PBKDF2 (`PasswordHasher` de ASP.NET Core Identity); nunca se devuelven en respuestas (`[JsonIgnore]`).
- Numeración de entidades: los objetos semilla usan IDs 1–3 (base) y 9001+ (prueba); los registros nuevos continúan la secuencia identity.

## Flujo de trabajo Git

- `feature/HU-XX` → desarrollo por historia de usuario.
- `develop` → rama de integración; se incorporan las features con merge `--no-ff`.
- `main` → versión estable; recibe `develop` cuando la integración compila y los endpoints están validados.
- Mensajes de commit con el formato `[HU-XX] Nombre: descripción`.