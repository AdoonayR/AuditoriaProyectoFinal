﻿using System;
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
                name: "Auditors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quimicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Packaging = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Lot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fifo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mixed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QcSeal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clean = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Auditor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Almacen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quimicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Aprobaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuimicoId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByIncoming = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedByStorage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedDateIncoming = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedDateStorage = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aprobaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aprobaciones_Quimicos_QuimicoId",
                        column: x => x.QuimicoId,
                        principalTable: "Quimicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Disposiciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuimicoId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comentarios = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoDmr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disposiciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disposiciones_Quimicos_QuimicoId",
                        column: x => x.QuimicoId,
                        principalTable: "Quimicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aprobaciones_QuimicoId",
                table: "Aprobaciones",
                column: "QuimicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Disposiciones_QuimicoId",
                table: "Disposiciones",
                column: "QuimicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aprobaciones");

            migrationBuilder.DropTable(
                name: "Auditors");

            migrationBuilder.DropTable(
                name: "Disposiciones");

            migrationBuilder.DropTable(
                name: "Quimicos");
        }
    }
}
