using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditoriaQuimicos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quimicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Packaging = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fifo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mixed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QcSeal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clean = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quimicos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quimicos");
        }
    }
}
