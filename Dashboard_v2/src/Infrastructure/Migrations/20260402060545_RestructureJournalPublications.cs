using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestructureJournalPublications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalGroup1Publications");

            migrationBuilder.DropColumn(
                name: "DataBase",
                table: "JournalPublications");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "JournalPublications");

            migrationBuilder.CreateTable(
                name: "Journals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ISSN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EISSN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    JournalPublicationId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journals_JournalPublications_JournalPublicationId",
                        column: x => x.JournalPublicationId,
                        principalTable: "JournalPublications",
                        principalColumn: "PublicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationDatabases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    JournalPublicationId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationDatabases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationDatabases_JournalPublications_JournalPublication~",
                        column: x => x.JournalPublicationId,
                        principalTable: "JournalPublications",
                        principalColumn: "PublicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScopusJournals",
                columns: table => new
                {
                    JournalId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Cuartil = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScopusJournals", x => x.JournalId);
                    table.ForeignKey(
                        name: "FK_ScopusJournals_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Journals_JournalPublicationId",
                table: "Journals",
                column: "JournalPublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationDatabases_JournalPublicationId",
                table: "PublicationDatabases",
                column: "JournalPublicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicationDatabases");

            migrationBuilder.DropTable(
                name: "ScopusJournals");

            migrationBuilder.DropTable(
                name: "Journals");

            migrationBuilder.AddColumn<string>(
                name: "DataBase",
                table: "JournalPublications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "JournalPublications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "JournalGroup1Publications",
                columns: table => new
                {
                    PublicationId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Cuartil = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalGroup1Publications", x => x.PublicationId);
                    table.ForeignKey(
                        name: "FK_JournalGroup1Publications_JournalPublications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "JournalPublications",
                        principalColumn: "PublicationId",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
