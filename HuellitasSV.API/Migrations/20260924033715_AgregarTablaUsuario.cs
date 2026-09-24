using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "cuenta",
                columns: new[] { "id_cuenta", "contrasena", "correo", "estado", "rol" },
                values: new object[,]
                {
                    { 9006L, "Usuario2026!", "marialopez@correo.com", "activo", "Usuario" },
                    { 9007L, "Usuario2026!", "carlosperez@correo.com", "inactivo", "Usuario" },
                    { 9008L, "Usuario2026!", "anagomez@correo.com", "bloqueado", "Usuario" }
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

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_cuenta",
                table: "usuario",
                column: "id_cuenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9006L);

            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9007L);

            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 9008L);
        }
    }
}
