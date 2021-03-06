// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VaultChanger.Data;

#nullable disable

namespace VaultChanger.Migrations
{
    [DbContext(typeof(VaultContext))]
    [Migration("20220710034530_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("VaultChanger.Models.Request", b =>
                {
                    b.Property<string>("VaultNamespace")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("MountPoint")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Path")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("VaultNamespace", "MountPoint", "Path", "Key");

                    b.ToTable("Requests");
                });
#pragma warning restore 612, 618
        }
    }
}
