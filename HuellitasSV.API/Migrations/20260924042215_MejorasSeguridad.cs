using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class MejorasSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    id_cuenta = table.Column<long>(type: "bigint", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    valor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    expira = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token_seguridad", x => x.id_token);
                });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1001L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1002L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1003L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9001L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9002L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9003L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9004L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9005L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEGpRavkRHJ/aY735e72svpCHsB4bya3yjaaoTgFIkSImDf/aT31gaWTTjv2yG1cSQg==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9006L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9007L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", true, false });

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9008L,
                columns: new[] { "contrasena", "correo_verificado", "dos_factores" },
                values: new object[] { "AQAAAAIAAYagAAAAEEwuG7WIzT50BNMbcOjCcrOXbVZYK+aPbwJW8gVrFg1VQ5QqPmopibuNtnP7+aCn5A==", true, false });

            migrationBuilder.CreateIndex(
                name: "IX_token_seguridad_id_cuenta",
                table: "token_seguridad",
                column: "id_cuenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1001L,
                column: "contrasena",
                value: "Refugio2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1002L,
                column: "contrasena",
                value: "Refugio2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1003L,
                column: "contrasena",
                value: "Refugio2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9001L,
                column: "contrasena",
                value: "Refugio2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9002L,
                column: "contrasena",
                value: "Refugio2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9003L,
                column: "contrasena",
                value: "Refugio2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9004L,
                column: "contrasena",
                value: "Refugio2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9005L,
                column: "contrasena",
                value: "Refugio2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9006L,
                column: "contrasena",
                value: "Usuario2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9007L,
                column: "contrasena",
                value: "Usuario2026!");

            migrationBuilder.UpdateData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9008L,
                column: "contrasena",
                value: "Usuario2026!");
        }
    }
}
