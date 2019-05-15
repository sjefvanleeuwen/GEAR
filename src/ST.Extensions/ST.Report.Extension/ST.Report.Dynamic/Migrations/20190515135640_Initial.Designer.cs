﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ST.Report.Dynamic.Data;

namespace ST.Report.Dynamic.Migrations
{
    [DbContext(typeof(DynamicReportDbContext))]
    [Migration("20190515135640_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ST.Report.Abstractions.Models.DynamicReportDbModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Author");

                    b.Property<DateTime>("Changed");

                    b.Property<int>("ChartType");

                    b.Property<string>("ColumnNames");

                    b.Property<DateTime>("Created");

                    b.Property<Guid>("DynamicReportFolderId");

                    b.Property<DateTime>("EndDateTime");

                    b.Property<string>("FiltersList");

                    b.Property<int>("GraphType");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedBy");

                    b.Property<string>("Name");

                    b.Property<DateTime>("StartDateTime");

                    b.Property<string>("TableName");

                    b.Property<Guid?>("TenantId");

                    b.Property<int>("TimeFrameEnum");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.HasIndex("DynamicReportFolderId");

                    b.ToTable("DynamicReports");
                });

            modelBuilder.Entity("ST.Report.Abstractions.Models.DynamicReportFolder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Author");

                    b.Property<DateTime>("Changed");

                    b.Property<DateTime>("Created");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedBy");

                    b.Property<string>("Name");

                    b.Property<Guid?>("TenantId");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.ToTable("DynamicReportsFolders");
                });

            modelBuilder.Entity("ST.Report.Abstractions.Models.DynamicReportDbModel", b =>
                {
                    b.HasOne("ST.Report.Abstractions.Models.DynamicReportFolder")
                        .WithMany("Reports")
                        .HasForeignKey("DynamicReportFolderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
