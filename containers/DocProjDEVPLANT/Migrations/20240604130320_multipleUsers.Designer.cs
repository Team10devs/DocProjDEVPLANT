﻿// <auto-generated />
using DocProjDEVPLANT.Repository.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DocProjDEVPLANT.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240604130320_multipleUsers")]
    partial class multipleUsers
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.Company.CompanyModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.Templates.PdfModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<int>("CurrentNumberOfUsers")
                        .HasColumnType("integer");

                    b.Property<string>("TemplateId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TemplateId");

                    b.ToTable("PdfModel");
                });

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.Templates.TemplateModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("CompanyId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("DocxFile")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TotalNumberOfUsers")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.User.UserModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CNP")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CompanyId")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("isEmail")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.Templates.PdfModel", b =>
                {
                    b.HasOne("DocProjDEVPLANT.Domain.Entities.Templates.TemplateModel", "Template")
                        .WithMany("GeneratedPdfs")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Template");
                });

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.Templates.TemplateModel", b =>
                {
                    b.HasOne("DocProjDEVPLANT.Domain.Entities.Company.CompanyModel", "Company")
                        .WithMany("Templates")
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Company");
                });

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.User.UserModel", b =>
                {
                    b.HasOne("DocProjDEVPLANT.Domain.Entities.Company.CompanyModel", "Company")
                        .WithMany("Users")
                        .HasForeignKey("CompanyId");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.Company.CompanyModel", b =>
                {
                    b.Navigation("Templates");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("DocProjDEVPLANT.Domain.Entities.Templates.TemplateModel", b =>
                {
                    b.Navigation("GeneratedPdfs");
                });
#pragma warning restore 612, 618
        }
    }
}
