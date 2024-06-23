using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocProjDEVPLANT.Migrations
{
    /// <inheritdoc />
    public partial class status_PDF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Pdfs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Pdfs");
        }
    }
}
