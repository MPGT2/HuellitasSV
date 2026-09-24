using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class DatosPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "cuenta",
                columns: new[] { "id_cuenta", "contrasena", "correo", "estado", "rol" },
                values: new object[,]
                {
                    { 9001L, "Refugio2026!", "losamigos.refugio@correo.com", "aprobado", "Refugio" },
                    { 9002L, "Refugio2026!", "hogarsantaana@correo.com", "aprobado", "Refugio" },
                    { 9003L, "Refugio2026!", "vidasonsonate@correo.com", "pendiente", "Refugio" },
                    { 9004L, "Refugio2026!", "patitasusulutan@correo.com", "pendiente", "Refugio" },
                    { 9005L, "Refugio2026!", "ayudaanimalchalate@correo.com", "rechazado", "Refugio" }
                });

            migrationBuilder.InsertData(
                table: "mascota",
                columns: new[] { "id_mascota", "edad_meses", "especie", "estado", "estado_salud", "fecha_registro", "id_refugio", "imagen_content_type", "imagen_data", "imagen_url", "nombre", "tamano" },
                values: new object[,]
                {
                    { 9001L, 48, "perro", "disponible", "sano", new DateTime(2026, 5, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Rocky II", "grande" },
                    { 9002L, 8, "gato", "disponible", "sano", new DateTime(2026, 5, 20, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Mia", "pequeño" },
                    { 9003L, 30, "perro", "reservada", "en_tratamiento", new DateTime(2026, 6, 10, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Bobby", "mediano" },
                    { 9004L, 18, "gato", "disponible", "crónico", new DateTime(2026, 6, 25, 10, 0, 0, 0, DateTimeKind.Utc), 2L, null, null, null, "Coco", "mediano" },
                    { 9005L, 60, "perro", "adoptada", "sano", new DateTime(2026, 7, 5, 10, 0, 0, 0, DateTimeKind.Utc), 2L, null, null, null, "Thor", "grande" }
                });

            migrationBuilder.InsertData(
                table: "refugio",
                columns: new[] { "id_refugio", "contacto", "departamento", "documentacion_url", "estado_aprobacion", "id_cuenta", "municipio", "nombre_organizacion" },
                values: new object[,]
                {
                    { 9001L, "7770-0001", "San Salvador", "https://losamigos.org/docs", "aprobado", 9001L, "San Salvador", "Refugio Los Amigos" },
                    { 9002L, "7770-0002", "Santa Ana", null, "aprobado", 9002L, "Santa Ana", "Hogar Animal Santa Ana" },
                    { 9003L, "7770-0003", "Sonsonate", null, "pendiente", 9003L, "Sonsonate", "Vida Animal Sonsonate" },
                    { 9004L, "7770-0004", "Usulután", "https://patitasusulutan.org/docs", "pendiente", 9004L, "Usulután", "Patitas de Usulután" },
                    { 9005L, "7770-0005", "Chalatenango", null, "rechazado", 9005L, "Chalatenango", "Ayuda Animal Chalatenango" }
                });

            migrationBuilder.InsertData(
                table: "mascota",
                columns: new[] { "id_mascota", "edad_meses", "especie", "estado", "estado_salud", "fecha_registro", "id_refugio", "imagen_content_type", "imagen_data", "imagen_url", "nombre", "tamano" },
                values: new object[,]
                {
                    { 9006L, 4, "gato", "disponible", "sano", new DateTime(2026, 7, 20, 10, 0, 0, 0, DateTimeKind.Utc), 9001L, null, null, "https://placekitten.com/400/300", "Nina", "pequeño" },
                    { 9007L, 10, "otro", "disponible", "discapacidad", new DateTime(2026, 8, 2, 10, 0, 0, 0, DateTimeKind.Utc), 9001L, null, null, null, "Simón", "pequeño" },
                    { 9008L, 24, "perro", "disponible", "sano", new DateTime(2026, 8, 15, 10, 0, 0, 0, DateTimeKind.Utc), 9002L, null, null, "https://placedog.net/500/400", "Duke", "grande" },
                    { 9009L, 14, "gato", "en_tratamiento", "sano", new DateTime(2026, 8, 28, 10, 0, 0, 0, DateTimeKind.Utc), 9002L, null, null, null, "Pelusa", "mediano" },
                    { 9010L, 36, "perro", "fallecida", "sano", new DateTime(2026, 9, 10, 10, 0, 0, 0, DateTimeKind.Utc), 9002L, null, null, null, "Zeus", "mediano" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9001L);

            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9002L);

            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9003L);

            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9004L);

            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9005L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9001L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9002L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9003L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9004L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9005L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9006L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9007L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9008L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9009L);

            migrationBuilder.DeleteData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9010L);

            migrationBuilder.DeleteData(
                table: "refugio",
                keyColumn: "id_refugio",
                keyValue: 9003L);

            migrationBuilder.DeleteData(
                table: "refugio",
                keyColumn: "id_refugio",
                keyValue: 9004L);

            migrationBuilder.DeleteData(
                table: "refugio",
                keyColumn: "id_refugio",
                keyValue: 9005L);

            migrationBuilder.DeleteData(
                table: "refugio",
                keyColumn: "id_refugio",
                keyValue: 9001L);

            migrationBuilder.DeleteData(
                table: "refugio",
                keyColumn: "id_refugio",
                keyValue: 9002L);
        }
    }
}
