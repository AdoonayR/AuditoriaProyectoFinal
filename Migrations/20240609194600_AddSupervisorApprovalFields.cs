using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditoriaQuimicos.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Lot",
                table: "Quimicos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByIncoming",
                table: "Quimicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByWarehouse",
                table: "Quimicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedByIncoming",
                table: "Quimicos");

            migrationBuilder.DropColumn(
                name: "ApprovedByWarehouse",
                table: "Quimicos");

            migrationBuilder.AlterColumn<int>(
                name: "Lot",
                table: "Quimicos",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
