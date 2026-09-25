using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "anuncio",
                columns: table => new
                {
                    id_anuncio = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_tienda = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    contacto_tienda = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    imagen_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    pago_confirmado = table.Column<bool>(type: "bit", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fecha_aprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anuncio", x => x.id_anuncio);
                });

            migrationBuilder.CreateTable(
                name: "cuenta",
                columns: table => new
                {
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    contrasena = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta", x => x.id_cuenta);
                });

            migrationBuilder.CreateTable(
                name: "refugio",
                columns: table => new
                {
                    id_refugio = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false),
                    nombre_organizacion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    municipio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    contacto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    documentacion_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    latitud = table.Column<double>(type: "float", nullable: true),
                    longitud = table.Column<double>(type: "float", nullable: true),
                    estado_aprobacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refugio", x => x.id_refugio);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "mascota",
                columns: table => new
                {
                    id_mascota = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_refugio = table.Column<long>(type: "bigint", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    especie = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    tamano = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    edad_meses = table.Column<int>(type: "int", nullable: false),
                    estado_salud = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    imagen_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    imagen_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    imagen_content_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mascota", x => x.id_mascota);
                    table.ForeignKey(
                        name: "FK_mascota_refugio_id_refugio",
                        column: x => x.id_refugio,
                        principalTable: "refugio",
                        principalColumn: "id_refugio",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "necesidad_donacion",
                columns: table => new
                {
                    id_necesidad = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_refugio = table.Column<long>(type: "bigint", nullable: false),
                    tipo_insumo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    cantidad_requerida = table.Column<decimal>(type: "decimal(10,2)", precision: 18, scale: 2, nullable: false),
                    cantidad_cubierta = table.Column<decimal>(type: "decimal(10,2)", precision: 18, scale: 2, nullable: false),
                    estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_necesidad_donacion", x => x.id_necesidad);
                    table.ForeignKey(
                        name: "FK_necesidad_donacion_refugio_id_refugio",
                        column: x => x.id_refugio,
                        principalTable: "refugio",
                        principalColumn: "id_refugio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    IdNotificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IdRefugio = table.Column<long>(type: "bigint", nullable: true),
                    IdUsuario = table.Column<long>(type: "bigint", nullable: true),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.IdNotificacion);
                    table.ForeignKey(
                        name: "FK_Notificaciones_refugio_IdRefugio",
                        column: x => x.IdRefugio,
                        principalTable: "refugio",
                        principalColumn: "id_refugio",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notificaciones_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportesAnimales",
                columns: table => new
                {
                    IdReporte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<long>(type: "bigint", nullable: false),
                    IdRefugio = table.Column<long>(type: "bigint", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Latitud = table.Column<double>(type: "float", nullable: false),
                    Longitud = table.Column<double>(type: "float", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesAnimales", x => x.IdReporte);
                    table.ForeignKey(
                        name: "FK_ReportesAnimales_refugio_IdRefugio",
                        column: x => x.IdRefugio,
                        principalTable: "refugio",
                        principalColumn: "id_refugio",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportesAnimales_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesAdopcion",
                columns: table => new
                {
                    IdSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMascota = table.Column<long>(type: "bigint", nullable: false),
                    IdUsuario = table.Column<long>(type: "bigint", nullable: false),
                    NombreContacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CorreoContacto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComentarioDecision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesAdopcion", x => x.IdSolicitud);
                    table.ForeignKey(
                        name: "FK_SolicitudesAdopcion_mascota_IdMascota",
                        column: x => x.IdMascota,
                        principalTable: "mascota",
                        principalColumn: "id_mascota",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesAdopcion_usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "cuenta",
                columns: new[] { "id_cuenta", "contrasena", "correo", "estado", "rol" },
                values: new object[,]
                {
                    { 1001L, "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", "contacto@huellitassv.org", "aprobado", "Refugio" },
                    { 1002L, "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", "adopciones@proteccionsv.org", "aprobado", "Refugio" },
                    { 1003L, "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", "info@alberguesm.org", "pendiente", "Refugio" },
                    { 9001L, "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", "losamigos.refugio@correo.com", "aprobado", "Refugio" },
                    { 9002L, "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", "hogarsantaana@correo.com", "aprobado", "Refugio" },
                    { 9003L, "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", "vidasonsonate@correo.com", "pendiente", "Refugio" },
                    { 9004L, "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", "patitasusulutan@correo.com", "pendiente", "Refugio" },
                    { 9005L, "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", "ayudaanimalchalate@correo.com", "rechazado", "Refugio" },
                    { 9006L, "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", "marialopez@correo.com", "activo", "Usuario" },
                    { 9007L, "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", "carlosperez@correo.com", "inactivo", "Usuario" },
                    { 9008L, "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", "anagomez@correo.com", "bloqueado", "Usuario" }
                });

            migrationBuilder.InsertData(
                table: "refugio",
                columns: new[] { "id_refugio", "contacto", "departamento", "documentacion_url", "estado_aprobacion", "id_cuenta", "latitud", "longitud", "municipio", "nombre_organizacion" },
                values: new object[,]
                {
                    { 1L, "contacto@huellitassv.org", "San Salvador", "https://huellitassv.org/docs", "aprobado", 1001L, null, null, "San Salvador", "Refugio Huellitas San Salvador" },
                    { 2L, "adopciones@proteccionsv.org", "La Libertad", null, "aprobado", 1002L, null, null, "Santa Tecla", "Protección Animal Santa Tecla" },
                    { 3L, "info@alberguesm.org", "San Miguel", "https://alberguesm.org/documentos", "pendiente", 1003L, null, null, "San Miguel", "Albergue Canino San Miguel" },
                    { 9001L, "7770-0001", "San Salvador", "https://losamigos.org/docs", "aprobado", 9001L, null, null, "San Salvador", "Refugio Los Amigos" },
                    { 9002L, "7770-0002", "Santa Ana", null, "aprobado", 9002L, null, null, "Santa Ana", "Hogar Animal Santa Ana" },
                    { 9003L, "7770-0003", "Sonsonate", null, "pendiente", 9003L, null, null, "Sonsonate", "Vida Animal Sonsonate" },
                    { 9004L, "7770-0004", "Usulután", "https://patitasusulutan.org/docs", "pendiente", 9004L, null, null, "Usulután", "Patitas de Usulután" },
                    { 9005L, "7770-0005", "Chalatenango", null, "rechazado", 9005L, null, null, "Chalatenango", "Ayuda Animal Chalatenango" }
                });

            migrationBuilder.InsertData(
                table: "usuario",
                columns: new[] { "id_usuario", "id_cuenta", "nombre" },
                values: new object[,]
                {
                    { 9001L, 9006L, "María López" },
                    { 9002L, 9007L, "Carlos Pérez" },
                    { 9003L, 9008L, "Ana Gómez" }
                });

            migrationBuilder.InsertData(
                table: "mascota",
                columns: new[] { "id_mascota", "edad_meses", "especie", "estado", "estado_salud", "fecha_registro", "id_refugio", "imagen_content_type", "imagen_data", "imagen_url", "nombre", "tamano" },
                values: new object[,]
                {
                    { 1L, 24, "perro", "disponible", "sano", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Firulais", "mediano" },
                    { 2L, 12, "gato", "disponible", "sano", new DateTime(2026, 2, 20, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Michi", "pequeño" },
                    { 3L, 36, "perro", "disponible", "en_tratamiento", new DateTime(2026, 3, 10, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Rex", "grande" },
                    { 4L, 18, "perro", "adoptada", "sano", new DateTime(2026, 1, 5, 10, 0, 0, 0, DateTimeKind.Utc), 2L, null, null, null, "Luna", "mediano" },
                    { 5L, 6, "gato", "disponible", "sano", new DateTime(2026, 4, 1, 10, 0, 0, 0, DateTimeKind.Utc), 2L, null, null, null, "Simba", "pequeño" },
                    { 6L, 48, "perro", "en_tratamiento", "crónico", new DateTime(2026, 2, 28, 10, 0, 0, 0, DateTimeKind.Utc), 3L, null, null, null, "Rocky", "grande" },
                    { 9001L, 48, "perro", "disponible", "sano", new DateTime(2026, 5, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Rocky II", "grande" },
                    { 9002L, 8, "gato", "disponible", "sano", new DateTime(2026, 5, 20, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Mia", "pequeño" },
                    { 9003L, 30, "perro", "reservada", "en_tratamiento", new DateTime(2026, 6, 10, 10, 0, 0, 0, DateTimeKind.Utc), 1L, null, null, null, "Bobby", "mediano" },
                    { 9004L, 18, "gato", "disponible", "crónico", new DateTime(2026, 6, 25, 10, 0, 0, 0, DateTimeKind.Utc), 2L, null, null, null, "Coco", "mediano" },
                    { 9005L, 60, "perro", "adoptada", "sano", new DateTime(2026, 7, 5, 10, 0, 0, 0, DateTimeKind.Utc), 2L, null, null, null, "Thor", "grande" },
                    { 9006L, 4, "gato", "disponible", "sano", new DateTime(2026, 7, 20, 10, 0, 0, 0, DateTimeKind.Utc), 9001L, null, null, "https://placekitten.com/400/300", "Nina", "pequeño" },
                    { 9007L, 10, "otro", "disponible", "discapacidad", new DateTime(2026, 8, 2, 10, 0, 0, 0, DateTimeKind.Utc), 9001L, null, null, null, "Simón", "pequeño" },
                    { 9008L, 24, "perro", "disponible", "sano", new DateTime(2026, 8, 15, 10, 0, 0, 0, DateTimeKind.Utc), 9002L, null, null, "https://placedog.net/500/400", "Duke", "grande" },
                    { 9009L, 14, "gato", "en_tratamiento", "sano", new DateTime(2026, 8, 28, 10, 0, 0, 0, DateTimeKind.Utc), 9002L, null, null, null, "Pelusa", "mediano" },
                    { 9010L, 36, "perro", "fallecida", "sano", new DateTime(2026, 9, 10, 10, 0, 0, 0, DateTimeKind.Utc), 9002L, null, null, null, "Zeus", "mediano" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_mascota_estado",
                table: "mascota",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_mascota_id_refugio",
                table: "mascota",
                column: "id_refugio");

            migrationBuilder.CreateIndex(
                name: "IX_necesidad_donacion_id_refugio",
                table: "necesidad_donacion",
                column: "id_refugio");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_IdRefugio",
                table: "Notificaciones",
                column: "IdRefugio");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_IdUsuario",
                table: "Notificaciones",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_refugio_estado_aprobacion",
                table: "refugio",
                column: "estado_aprobacion");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesAnimales_IdRefugio",
                table: "ReportesAnimales",
                column: "IdRefugio");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesAnimales_IdUsuario",
                table: "ReportesAnimales",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAdopcion_IdMascota",
                table: "SolicitudesAdopcion",
                column: "IdMascota");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAdopcion_IdUsuario",
                table: "SolicitudesAdopcion",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_cuenta",
                table: "usuario",
                column: "id_cuenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anuncio");

            migrationBuilder.DropTable(
                name: "cuenta");

            migrationBuilder.DropTable(
                name: "necesidad_donacion");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "ReportesAnimales");

            migrationBuilder.DropTable(
                name: "SolicitudesAdopcion");

            migrationBuilder.DropTable(
                name: "mascota");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "refugio");
        }
    }
}
