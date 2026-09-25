using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCuentaAdminYSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "cuenta",
                columns: new[] { "id_cuenta", "contrasena", "correo", "estado", "rol" },
                values: new object[] { 1L, "AQAAAAIAAYagAAAAECJkUTNTpDEXyGK0S6lDi/pF5uh9pDEYK2H/U3+h/ciWOS/vclz8lBWHa+9Vy2kCAg==", "admin@huellitassv.org", "aprobado", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "cuenta",
                keyColumn: "id_cuenta",
                keyValue: 1L);
        }
    }
}
