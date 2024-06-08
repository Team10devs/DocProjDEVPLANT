using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocProjDEVPLANT.Migrations
{
    /// <inheritdoc />
    public partial class addedlistofuserstopdfandjsontousers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfModelId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserData",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PdfModelId",
                table: "Users",
                column: "PdfModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Pdfs_PdfModelId",
                table: "Users",
                column: "PdfModelId",
                principalTable: "Pdfs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Pdfs_PdfModelId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PdfModelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PdfModelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserData",
                table: "Users");
        }
    }
}
