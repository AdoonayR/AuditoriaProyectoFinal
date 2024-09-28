using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditoriaQuimicos.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApprovedDate",
                table: "Aprobaciones",
                newName: "ApprovedDateStorage");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                table: "Aprobaciones",
                newName: "ApprovedByStorage");

            migrationBuilder.RenameColumn(
                name: "ApprovalType",
                table: "Aprobaciones",
                newName: "ApprovedByIncoming");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDateIncoming",
                table: "Aprobaciones",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedDateIncoming",
                table: "Aprobaciones");

            migrationBuilder.RenameColumn(
                name: "ApprovedDateStorage",
                table: "Aprobaciones",
                newName: "ApprovedDate");

            migrationBuilder.RenameColumn(
                name: "ApprovedByStorage",
                table: "Aprobaciones",
                newName: "ApprovedBy");

            migrationBuilder.RenameColumn(
                name: "ApprovedByIncoming",
                table: "Aprobaciones",
                newName: "ApprovalType");
        }
    }
}
