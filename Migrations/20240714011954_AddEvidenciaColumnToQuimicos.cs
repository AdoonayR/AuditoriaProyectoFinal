using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditoriaQuimicos.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenciaColumnToQuimicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Evidencia",
                table: "Quimicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Evidencia",
                table: "Quimicos");
        }
    }
}
