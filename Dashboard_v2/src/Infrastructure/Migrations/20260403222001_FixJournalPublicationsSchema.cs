using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixJournalPublicationsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The JournalPublications table was created by an older schema that stored Name and
            // DataBase in separate Journals/PublicationDatabases related tables. This migration
            // adds those columns directly on the table (as the current EF model expects) and
            // drops the obsolete legacy tables.
            migrationBuilder.Sql(@"
                -- Add Name and DataBase columns that the current EF model expects
                ALTER TABLE ""JournalPublications""
                    ADD COLUMN IF NOT EXISTS ""Name"" character varying(500) NOT NULL DEFAULT '';

                ALTER TABLE ""JournalPublications""
                    ADD COLUMN IF NOT EXISTS ""DataBase"" character varying(500) NOT NULL DEFAULT '';

                -- Remove defaults (EF manages nullability/defaults via code)
                ALTER TABLE ""JournalPublications"" ALTER COLUMN ""Name"" DROP DEFAULT;
                ALTER TABLE ""JournalPublications"" ALTER COLUMN ""DataBase"" DROP DEFAULT;

                -- Drop legacy tables (old schema stored journal name/database in separate tables)
                DROP TABLE IF EXISTS ""ScopusJournals"";
                DROP TABLE IF EXISTS ""PublicationDatabases"";
                DROP TABLE IF EXISTS ""Journals"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""JournalPublications"" DROP COLUMN IF EXISTS ""Name"";
                ALTER TABLE ""JournalPublications"" DROP COLUMN IF EXISTS ""DataBase"";
            ");
        }
    }
}
