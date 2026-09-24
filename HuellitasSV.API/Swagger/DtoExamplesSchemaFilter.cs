// [HU-10] Michael Menendez: Estructura base - Filtro de ejemplos para Swagger.
// Evita respuestas 400 en "Try it out" al proponer cuerpos de solicitud válidos para cada DTO.

namespace HuellitasSV.API.Swagger;

using HuellitasSV.API.DTOs;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Define ejemplos válidos en Swagger para los DTOs, de modo que "Try it out"
/// proponga cuerpos de solicitud correctos y se eviten respuestas 400 por ejemplo inválido.
/// </summary>
public class DtoExamplesSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Asigna el ejemplo de solicitud acorde al tipo de DTO siendo documentado.
    /// </summary>
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete)
            return;

        if (context.Type == typeof(RegistrarMascotaDto))
        {
            concrete.Example = new System.Text.Json.Nodes.JsonObject
            {
                ["idRefugio"] = 1,
                ["nombre"] = "Firulais",
                ["especie"] = "perro",
                ["tamano"] = "mediano",
                ["edadMeses"] = 24,
                ["estadoSalud"] = "sano"
            };
        }
        else if (context.Type == typeof(ActualizarMascotaDto))
        {
            concrete.Example = new System.Text.Json.Nodes.JsonObject
            {
                ["nombre"] = "Firulais",
                ["estado"] = "adoptada",
                ["justificacionCambioEstado"] = "Adoptada por una familia responsable."
            };
        }
        else if (context.Type == typeof(HuellitasSV.API.Controllers.RegistroRefugioDto))
        {
            concrete.Example = new System.Text.Json.Nodes.JsonObject
            {
                ["nombreOrganizacion"] = "Refugio Patitas Soyapango",
                ["correo"] = "patitas@refugio.org",
                ["contrasena"] = "Refugio2026!",
                ["departamento"] = "San Salvador",
                ["municipio"] = "Soyapango",
                ["contacto"] = "2222-0000"
            };
        }
        else if (context.Type == typeof(HuellitasSV.API.Controllers.LoginRefugioDto))
        {
            concrete.Example = new System.Text.Json.Nodes.JsonObject
            {
                ["correo"] = "contacto@huellitassv.org",
                ["contrasena"] = "Refugio2026!"
            };
        }
        else if (context.Type == typeof(HuellitasSV.API.Controllers.RegistroUsuarioDto))
        {
            concrete.Example = new System.Text.Json.Nodes.JsonObject
            {
                ["nombre"] = "María López",
                ["correo"] = "maria.lopez@correo.com",
                ["contrasena"] = "Usuario2026!"
            };
        }
        else if (context.Type == typeof(HuellitasSV.API.Controllers.LoginUsuarioDto))
        {
            concrete.Example = new System.Text.Json.Nodes.JsonObject
            {
                ["correo"] = "marialopez@correo.com",
                ["contrasena"] = "Usuario2026!"
            };
        }
    }
}