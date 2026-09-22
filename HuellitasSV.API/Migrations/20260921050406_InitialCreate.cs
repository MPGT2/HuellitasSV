using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refugio",
                columns: table => new
                {
                    id_refugio = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false),
                    nombre_organizacion = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    departamento = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    municipio = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    contacto = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    documentacion_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    estado_aprobacion = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refugio", x => x.id_refugio);
                });

            migrationBuilder.CreateTable(
                name: "mascota",
                columns: table => new
                {
                    id_mascota = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_refugio = table.Column<long>(type: "bigint", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    especie = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    tamano = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    edad_meses = table.Column<int>(type: "int", nullable: false),
                    estado_salud = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    imagen_url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    imagen_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    imagen_content_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_mascota_estado",
                table: "mascota",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_mascota_id_refugio",
                table: "mascota",
                column: "id_refugio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mascota");

            migrationBuilder.DropTable(
                name: "refugio");
        }
    }
}
