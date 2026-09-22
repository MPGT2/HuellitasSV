using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "refugio",
                columns: new[] { "id_refugio", "id_cuenta", "nombre_organizacion", "departamento", "municipio", "contacto", "documentacion_url", "estado_aprobacion" },
                values: new object[,]
                {
                    { 1L, 1001L, "Refugio Huellitas San Salvador", "San Salvador", "San Salvador", "contacto@huellitassv.org", "https://huellitassv.org/docs", "aprobado" },
                    { 2L, 1002L, "Protección Animal Santa Tecla", "La Libertad", "Santa Tecla", "adopciones@proteccionsv.org", null, "aprobado" },
                    { 3L, 1003L, "Albergue Canino San Miguel", "San Miguel", "San Miguel", "info@alberguesm.org", "https://alberguesm.org/documentos", "pendiente" }
                });

            migrationBuilder.InsertData(
                table: "mascota",
                columns: new[] { "id_mascota", "id_refugio", "nombre", "especie", "tamano", "edad_meses", "estado_salud", "estado", "fecha_registro", "imagen_url", "imagen_data", "imagen_content_type" },
                values: new object[,]
                {
                    { 1L, 1L, "Firulais", "perro", "mediano", 24, "sano", "disponible", new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified), null, null, null },
                    { 2L, 1L, "Michi", "gato", "pequeño", 12, "sano", "disponible", new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Unspecified), null, null, null },
                    { 3L, 1L, "Rex", "perro", "grande", 36, "en_tratamiento", "disponible", new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Unspecified), null, null, null },
                    { 4L, 2L, "Luna", "perro", "mediano", 18, "sano", "adoptada", new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Unspecified), null, null, null },
                    { 5L, 2L, "Simba", "gato", "pequeño", 6, "sano", "disponible", new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Unspecified), null, null, null },
                    { 6L, 3L, "Rocky", "perro", "grande", 48, "crónico", "en_tratamiento", new DateTime(2026, 2, 28, 10, 0, 0, DateTimeKind.Unspecified), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "mascota", keyColumn: "id_mascota", keyValues: new object[] { 1L, 2L, 3L, 4L, 5L, 6L });
            migrationBuilder.DeleteData(table: "refugio", keyColumn: "id_refugio", keyValues: new object[] { 1L, 2L, 3L });
        }
    }
}
