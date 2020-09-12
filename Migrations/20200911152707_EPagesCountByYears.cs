using Microsoft.EntityFrameworkCore.Migrations;
using stranitza.Models.Database;

namespace stranitza.Migrations
{
    public partial class EPagesCountByYears : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW EPagesCountByYears AS
	SELECT x.ReleaseYear as Year, COUNT(*) as Count 
	FROM StranitzaEPages x
	GROUP BY x.ReleaseYear
	ORDER BY x.ReleaseYear DESC");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW EPagesCountByYears");
        }
    }
}
