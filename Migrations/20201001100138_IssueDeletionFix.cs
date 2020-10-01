using Microsoft.EntityFrameworkCore.Migrations;

namespace stranitza.Migrations
{
    public partial class IssueDeletionFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_PdfFilePreviewId",
                table: "StranitzaIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_PdfFileReducedId",
                table: "StranitzaIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_ZipFileId",
                table: "StranitzaIssues");

            migrationBuilder.AddForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_PdfFilePreviewId",
                table: "StranitzaIssues",
                column: "PdfFilePreviewId",
                principalTable: "StranitzaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_PdfFileReducedId",
                table: "StranitzaIssues",
                column: "PdfFileReducedId",
                principalTable: "StranitzaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_ZipFileId",
                table: "StranitzaIssues",
                column: "ZipFileId",
                principalTable: "StranitzaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_PdfFilePreviewId",
                table: "StranitzaIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_PdfFileReducedId",
                table: "StranitzaIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_ZipFileId",
                table: "StranitzaIssues");

            migrationBuilder.AddForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_PdfFilePreviewId",
                table: "StranitzaIssues",
                column: "PdfFilePreviewId",
                principalTable: "StranitzaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_PdfFileReducedId",
                table: "StranitzaIssues",
                column: "PdfFileReducedId",
                principalTable: "StranitzaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StranitzaIssues_StranitzaFiles_ZipFileId",
                table: "StranitzaIssues",
                column: "ZipFileId",
                principalTable: "StranitzaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
