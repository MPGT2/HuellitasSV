using System.Text;
using System.Text.Json.Serialization;
using HuellitasSV.API.Data;
using HuellitasSV.API.Security;
using HuellitasSV.API.Services;
using HuellitasSV.API.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// La API trabaja siempre con formato invariante (punto decimal). Sin esto, el
// model binding de los formularios (multipart) usa la cultura del sistema y una
// latitud como "13.6929" se interpreta como 136929 (miles), fuera de rango.
System.Globalization.CultureInfo.DefaultThreadCurrentCulture =
    System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture =
    System.Globalization.CultureInfo.InvariantCulture;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Los estados se serializan como texto ("Pendiente", "Aprobada", "Atendido") en vez de números,
        // igual que los estados de mascota del modelo del equipo.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Formato de error de validación uniforme para todos los endpoints: { "errores": [ ... ] }.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(entry => entry.Value != null && entry.Value.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "El valor enviado no es válido."
                    : error.ErrorMessage)
                .ToList();

            return new BadRequestObjectResult(new { errores });
        };
    });

// Permite el consumo de la API desde el front-end de la aplicación.
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HuellitasSV API",
        Version = "v1",
        Description = "API de mascotas y refugios — HuellitasSV"
    });

    // Ejemplos válidos para los DTOs en la documentación interactiva.
    options.SchemaFilter<DtoExamplesSchemaFilter>();

    // [SEGURIDAD] Botón "Authorize" en Swagger: permite pegar el JWT para probar endpoints protegidos.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT (solo el token, sin el prefijo Bearer)."
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
    });

    // Documentación XML de los comentarios de código.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// BD en MonsterASP: pega la cadena en appsettings.json → ConnectionStrings:DefaultConnection
// (tambien se puede sobreescribir en Render con ConnectionStrings__DefaultConnection).
// EnableRetryOnFailure absorbe los cortes momentaneos de redtipicos de un
// contenedor en la nube; el timeout sube a 60 s porque la BD esta en otro pais.
//
// MonsterASP usa un certificado que no proviene de una autoridad publica, por eso
// la cadena incluye TrustServerCertificate=True (forma que exige su documentacion).
// Si algun dia el handshake TLS falla en Linux, la causa no es la cadena: basta con
// definir ConnectionStrings__DefaultConnection en Render usando Encrypt=False.
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var configuracion = serviceProvider.GetRequiredService<IConfiguration>();
    var cadena = configuracion.GetConnectionString("DefaultConnection");

    options.UseSqlServer(cadena, sql =>
    {
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        sql.CommandTimeout(60);
    });
});

// Acceso al HttpContext para resolver los claims del token una sola vez (ICurrentUserService).
builder.Services.AddHttpContextAccessor();

// [SEGURIDAD] Autenticación con JWT: los endpoints de gestión exigen un token firmado
// emitido por AuthController/UsuariosController/RefugiosController con el rol de la cuenta.
builder.Services.AddScoped<JwtTokenService>();

// Capa de servicios: lógica compartida entre controladores.
// Scoped porque todos usan ApplicationDbContext, que también es scoped.
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAccountStatePolicy, AccountStatePolicy>();
builder.Services.AddScoped<ICalificacionService, CalificacionService>();

// Almacenamiento de archivos subidos (imágenes de mascotas/reportes y documentos de refugio).
builder.Services.AddScoped<IArchivoService, ArchivoService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Captura cualquier excepcion no controlada y la deja escrita en el log del
// contenedor. Sin esto un fallo de base de datos devolvia un 500 sin detalle
// y era imposible saber desde Render que estaba pasando.
app.Use(async (contexto, siguiente) =>
{
    try
    {
        await siguiente();
    }
    catch (Exception excepcion)
    {
        app.Logger.LogError(
            excepcion,
            "Error no controlado al procesar {Metodo} {Ruta}",
            contexto.Request.Method,
            contexto.Request.Path);

        if (contexto.Response.HasStarted)
        {
            throw;
        }

        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        contexto.Response.ContentType = "application/json; charset=utf-8";

        // El detalle solo se devuelve en Development. En otros entornos el
        // cliente recibe un mensaje generico y el detalle queda en el log.
        if (app.Environment.IsDevelopment())
        {
            await contexto.Response.WriteAsJsonAsync(new
            {
                error = "Error interno del servidor.",
                tipo = excepcion.GetType().Name,
                detalle = excepcion.Message
            });
        }
        else
        {
            await contexto.Response.WriteAsJsonAsync(new { error = "Error interno del servidor." });
        }
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HuellitasSV API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHttpsRedirection();
}

// [HU-03] Sirve las fotos subidas en wwwroot/imagenes/mascotas (ImagenUrl relativo).
app.UseStaticFiles();

app.UseCors("PermitirFrontend");

// [SEGURIDAD] Orden obligatorio: autenticación (valida el JWT) y luego autorización ([Authorize]/[AllowAnonymous]).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();