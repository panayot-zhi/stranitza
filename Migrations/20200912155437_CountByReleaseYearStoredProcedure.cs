using Microsoft.EntityFrameworkCore.Migrations;

namespace stranitza.Migrations
{
    public partial class CountByReleaseYearStoredProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELIMITER $$
CREATE PROCEDURE `CountByReleaseYear`(IN queryType INT)
BEGIN
	CASE queryType
		WHEN 1 THEN -- Sources
			SELECT x.ReleaseYear as Year, COUNT(*) as Count 
			FROM StranitzaSources x
			GROUP BY x.ReleaseYear
			ORDER BY x.ReleaseYear DESC;
		WHEN 2 THEN -- EPages
			SELECT x.ReleaseYear as Year, COUNT(*) as Count 
			FROM StranitzaEPages x
			GROUP BY x.ReleaseYear
			ORDER BY x.ReleaseYear DESC;
		WHEN 3 THEN -- Issues (all)
			SELECT x.ReleaseYear as Year, COUNT(*) as Count 
			FROM StranitzaIssues x
			GROUP BY x.ReleaseYear
			ORDER BY x.ReleaseYear DESC;
		WHEN 4 THEN -- Issues (available)
			SELECT x.ReleaseYear as Year, COUNT(*) as Count 
			FROM StranitzaIssues x
			WHERE x.IsAvailable = 1
			GROUP BY x.ReleaseYear
			ORDER BY x.ReleaseYear DESC;
	END CASE;
END$$

DELIMITER ;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE `CountByReleaseYear`;");
        }
    }
}
