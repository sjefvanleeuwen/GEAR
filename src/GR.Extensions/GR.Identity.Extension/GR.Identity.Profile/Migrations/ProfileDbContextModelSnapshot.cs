﻿// <auto-generated />
using System;
using GR.Identity.Profile.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GR.Identity.Profile.Migrations
{
    [DbContext(typeof(ProfileDbContext))]
    partial class ProfileDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Identity")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("GR.Identity.Profile.Abstractions.Models.AddressModels.Address", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AddressLine1")
                        .HasMaxLength(450);

                    b.Property<string>("AddressLine2")
                        .HasMaxLength(450);

                    b.Property<string>("Author");

                    b.Property<DateTime>("Changed");

                    b.Property<string>("ContactName")
                        .HasMaxLength(450);

                    b.Property<string>("CountryId")
                        .IsRequired()
                        .HasMaxLength(450);

                    b.Property<DateTime>("Created");

                    b.Property<Guid?>("DistrictId");

                    b.Property<bool>("IsDefault");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedBy");

                    b.Property<string>("Phone")
                        .HasMaxLength(450);

                    b.Property<Guid>("StateOrProvinceId");

                    b.Property<Guid?>("TenantId");

                    b.Property<Guid>("UserId");

                    b.Property<int>("Version");

                    b.Property<string>("ZipCode")
                        .HasMaxLength(450);

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.HasIndex("DistrictId");

                    b.HasIndex("StateOrProvinceId");

                    b.HasIndex("UserId");

                    b.ToTable("UserAddresses");
                });

            modelBuilder.Entity("GR.Identity.Profile.Abstractions.Models.Profile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Author");

                    b.Property<DateTime>("Changed");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Description");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("ModifiedBy");

                    b.Property<string>("Name");

                    b.Property<int>("ProfileLevel");

                    b.Property<Guid?>("TenantId");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("GR.Identity.Profile.Abstractions.Models.RoleProfile", b =>
                {
                    b.Property<Guid>("RoleId");

                    b.Property<Guid>("ProfileId");

                    b.HasKey("RoleId", "ProfileId");

                    b.HasIndex("ProfileId");

                    b.ToTable("RoleProfiles");
                });

            modelBuilder.Entity("GR.Identity.Profile.Abstractions.Models.UserProfile", b =>
                {
                    b.Property<Guid>("UserId");

                    b.Property<Guid>("RoleProfileId");

                    b.Property<Guid?>("RoleProfileProfileId");

                    b.Property<Guid?>("RoleProfileRoleId");

                    b.HasKey("UserId", "RoleProfileId");

                    b.HasIndex("UserId");

                    b.HasIndex("RoleProfileRoleId", "RoleProfileProfileId");

                    b.ToTable("UserProfiles");
                });

            modelBuilder.Entity("GR.Identity.Profile.Abstractions.Models.RoleProfile", b =>
                {
                    b.HasOne("GR.Identity.Profile.Abstractions.Models.Profile", "Profile")
                        .WithMany()
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GR.Identity.Profile.Abstractions.Models.UserProfile", b =>
                {
                    b.HasOne("GR.Identity.Profile.Abstractions.Models.RoleProfile", "RoleProfile")
                        .WithMany()
                        .HasForeignKey("RoleProfileRoleId", "RoleProfileProfileId");
                });
#pragma warning restore 612, 618
        }
    }
}
