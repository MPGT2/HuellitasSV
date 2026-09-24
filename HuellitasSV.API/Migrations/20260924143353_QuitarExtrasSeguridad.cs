using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class QuitarExtrasSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "token_seguridad");

            migrationBuilder.DropColumn(
                name: "correo_verificado",
                table: "cuenta");

            migrationBuilder.DropColumn(
                name: "dos_factores",
                table: "cuenta");

            migrationBuilder.UpdateData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9002L,
                column: "tamano",
                value: "pequeÃ±o");

            migrationBuilder.UpdateData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9004L,
                column: "estado_salud",
                value: "crÃ³nico");

            migrationBuilder.UpdateData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9006L,
                column: "tamano",
                value: "pequeÃ±o");

            migrationBuilder.UpdateData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9007L,
                columns: new[] { "nombre", "tamano" },
                values: new object[] { "SimÃ³n", "pequeÃ±o" });

            migrationBuilder.UpdateData(
                table: "refugio",
                keyColumn: "id_refugio",
                keyValue: 9004L,
                columns: new[] { "departamento", "municipio", "nombre_organizacion" },
                values: new object[] { "UsulutÃ¡n", "UsulutÃ¡n", "Patitas de UsulutÃ¡n" });

            migrationBuilder.UpdateData(
                table: "usuario",
                keyColumn: "id_usuario",
                keyValue: 9001L,
                column: "nombre",
                value: "MarÃ­a LÃ³pez");

            migrationBuilder.UpdateData(
                table: "usuario",
                keyColumn: "id_usuario",
                keyValue: 9002L,
                column: "nombre",
                value: "Carlos PÃ©rez");

            migrationBuilder.UpdateData(
                table: "usuario",
                keyColumn: "id_usuario",
                keyValue: 9003L,
                column: "nombre",
                value: "Ana GÃ³mez");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "correo_verificado",
                table: "cuenta",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "dos_factores",
                table: "cuenta",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "token_seguridad",
                columns: table => new
                {
                    id_token = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    expira = table.Column<DateTime>(type: "datetime2", nullable: false),
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    valor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token_seguridad", x => x.id_token);
                });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1001L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1002L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1003L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9001L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9002L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9003L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9004L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9005L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9006L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9007L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9008L,
                columns: new[] { "correo_verificado", "dos_factores" },
                values: new object[] { true, false });

            migrationBuilder.UpdateData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9002L,
                column: "tamano",
                value: "pequeño");

            migrationBuilder.UpdateData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9004L,
                column: "estado_salud",
                value: "crónico");

            migrationBuilder.UpdateData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9006L,
                column: "tamano",
                value: "pequeño");

            migrationBuilder.UpdateData(
                table: "mascota",
                keyColumn: "id_mascota",
                keyValue: 9007L,
                columns: new[] { "nombre", "tamano" },
                values: new object[] { "Simón", "pequeño" });

            migrationBuilder.UpdateData(
                table: "refugio",
                keyColumn: "id_refugio",
                keyValue: 9004L,
                columns: new[] { "departamento", "municipio", "nombre_organizacion" },
                values: new object[] { "Usulután", "Usulután", "Patitas de Usulután" });

            migrationBuilder.UpdateData(
                table: "usuario",
                keyColumn: "id_usuario",
                keyValue: 9001L,
                column: "nombre",
                value: "María López");

            migrationBuilder.UpdateData(
                table: "usuario",
                keyColumn: "id_usuario",
                keyValue: 9002L,
                column: "nombre",
                value: "Carlos Pérez");

            migrationBuilder.UpdateData(
                table: "usuario",
                keyColumn: "id_usuario",
                keyValue: 9003L,
                column: "nombre",
                value: "Ana Gómez");

            migrationBuilder.CreateIndex(
                name: "IX_token_seguridad_id_cuenta",
                table: "token_seguridad",
                column: "id_cuenta");
        }
    }
}
