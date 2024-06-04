using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocProjDEVPLANT.Migrations
{
    /// <inheritdoc />
    public partial class multipleUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalNumberOfUsers",
                table: "Templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentNumberOfUsers",
                table: "PdfModel",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalNumberOfUsers",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CurrentNumberOfUsers",
                table: "PdfModel");
        }
    }
}
