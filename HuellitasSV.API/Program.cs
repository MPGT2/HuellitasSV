// [HU-10] Michael Menendez: Estructura base - Host de la API.
// Configura controladores, Swagger, CORS, formato de error 400 uniforme y el contexto EF Core.

using System.Text.Json.Serialization;
using HuellitasSV.API.Data;
using HuellitasSV.API.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

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

    // Documentación XML de los comentarios de código.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

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

app.UseCors("PermitirFrontend");

app.MapControllers();

// Aplica las migraciones (crea la BD, tablas y datos semilla) automáticamente al iniciar la API.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();