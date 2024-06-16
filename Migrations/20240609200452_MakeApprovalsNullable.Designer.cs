﻿// <auto-generated />
using System;
using AuditoriaQuimicos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AuditoriaQuimicos.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240609200452_MakeApprovalsNullable")]
    partial class MakeApprovalsNullable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AuditoriaQuimicos.Models.Auditor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("EmployeeNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Auditors");
                });

            modelBuilder.Entity("AuditoriaQuimicos.Models.Quimico", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ApprovedByIncoming")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ApprovedByWarehouse")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Auditor")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Clean")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Comments")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Expiration")
                        .HasColumnType("datetime2");

                    b.Property<string>("Fifo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Lot")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Mixed")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Packaging")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PartNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("QcSeal")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Result")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Quimicos");
                });
#pragma warning restore 612, 618
        }
    }
}
