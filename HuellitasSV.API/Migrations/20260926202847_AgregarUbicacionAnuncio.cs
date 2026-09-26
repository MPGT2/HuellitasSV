using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellitasSV.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUbicacionAnuncio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitud",
                table: "anuncio",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitud",
                table: "anuncio",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitud",
                table: "anuncio");

            migrationBuilder.DropColumn(
                name: "longitud",
                table: "anuncio");
        }
    }
}
