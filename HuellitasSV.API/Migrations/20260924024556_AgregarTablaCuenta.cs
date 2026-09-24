using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaCuenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "cuenta",
                columns: new[] { "id_cuenta", "contrasena", "correo", "estado", "rol" },
                values: new object[,]
                {
                    { 1001L, "Refugio2026!", "contacto@huellitassv.org", "aprobado", "Refugio" },
                    { 1002L, "Refugio2026!", "adopciones@proteccionsv.org", "aprobado", "Refugio" },
                    { 1003L, "Refugio2026!", "info@alberguesm.org", "pendiente", "Refugio" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_cuenta_correo",
                table: "cuenta",
                column: "correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cuenta");
        }
    }
}
