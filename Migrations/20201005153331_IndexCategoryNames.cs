using Microsoft.EntityFrameworkCore.Migrations;

namespace stranitza.Migrations
{
    public partial class IndexCategoryNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StranitzaCategories_Name",
                table: "StranitzaCategories",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StranitzaCategories_Name",
                table: "StranitzaCategories");
        }
    }
}
