using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropJournalPublicationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""JournalPublications"" DROP COLUMN IF EXISTS ""Name"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""JournalPublications""
                    ADD COLUMN IF NOT EXISTS ""Name"" character varying(500) NOT NULL DEFAULT '';
                ALTER TABLE ""JournalPublications"" ALTER COLUMN ""Name"" DROP DEFAULT;
            ");
        }
    }
}
