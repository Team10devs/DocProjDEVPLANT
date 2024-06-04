using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocProjDEVPLANT.Migrations
{
    /// <inheritdoc />
    public partial class addedpdfdbset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PdfModel_Templates_TemplateId",
                table: "PdfModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PdfModel",
                table: "PdfModel");

            migrationBuilder.RenameTable(
                name: "PdfModel",
                newName: "Pdfs");

            migrationBuilder.RenameIndex(
                name: "IX_PdfModel_TemplateId",
                table: "Pdfs",
                newName: "IX_Pdfs_TemplateId");

            migrationBuilder.AlterColumn<string>(
                name: "TemplateId",
                table: "Pdfs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pdfs",
                table: "Pdfs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pdfs_Templates_TemplateId",
                table: "Pdfs",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pdfs_Templates_TemplateId",
                table: "Pdfs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pdfs",
                table: "Pdfs");

            migrationBuilder.RenameTable(
                name: "Pdfs",
                newName: "PdfModel");

            migrationBuilder.RenameIndex(
                name: "IX_Pdfs_TemplateId",
                table: "PdfModel",
                newName: "IX_PdfModel_TemplateId");

            migrationBuilder.AlterColumn<string>(
                name: "TemplateId",
                table: "PdfModel",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PdfModel",
                table: "PdfModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PdfModel_Templates_TemplateId",
                table: "PdfModel",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
