using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CuartilToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guard: AddPublicationSpecializations may be recorded as applied but tables were never
            // physically created due to a DB/history mismatch. Create them if they don't exist,
            // using the final schema (Cuartil as varchar). If they already exist with an integer
            // Cuartil, the ALTER below will convert it.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""IndexedPublications"" (
                    ""PublicationId"" character varying(450) NOT NULL,
                    ""Index"" text NOT NULL,
                    CONSTRAINT ""PK_IndexedPublications"" PRIMARY KEY (""PublicationId""),
                    CONSTRAINT ""FK_IndexedPublications_Publications_PublicationId""
                        FOREIGN KEY (""PublicationId"") REFERENCES ""Publications"" (""Id"")
                        ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS ""JournalPublications"" (
                    ""PublicationId"" character varying(450) NOT NULL,
                    ""Name"" character varying(500) NOT NULL,
                    ""DataBase"" character varying(500) NOT NULL,
                    ""Group"" integer NOT NULL,
                    CONSTRAINT ""PK_JournalPublications"" PRIMARY KEY (""PublicationId""),
                    CONSTRAINT ""FK_JournalPublications_Publications_PublicationId""
                        FOREIGN KEY (""PublicationId"") REFERENCES ""Publications"" (""Id"")
                        ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS ""JournalGroup1Publications"" (
                    ""PublicationId"" character varying(450) NOT NULL,
                    ""Cuartil"" character varying(10) NOT NULL,
                    CONSTRAINT ""PK_JournalGroup1Publications"" PRIMARY KEY (""PublicationId""),
                    CONSTRAINT ""FK_JournalGroup1Publications_JournalPublications_PublicationId""
                        FOREIGN KEY (""PublicationId"") REFERENCES ""JournalPublications"" (""PublicationId"")
                        ON DELETE CASCADE
                );
            ");

            // If the table already existed with an integer Cuartil column, convert it to varchar.
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'JournalGroup1Publications'
                          AND column_name = 'Cuartil'
                          AND data_type = 'integer'
                    ) THEN
                        ALTER TABLE ""JournalGroup1Publications""
                        ALTER COLUMN ""Cuartil"" TYPE character varying(10) USING ""Cuartil""::text;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Cuartil",
                table: "JournalGroup1Publications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }
    }
}
