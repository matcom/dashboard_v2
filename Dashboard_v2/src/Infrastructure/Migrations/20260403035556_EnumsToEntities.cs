using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnumsToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublicationType",
                table: "Publications",
                newName: "PublicationTypeId");

            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "Events",
                newName: "EventTypeId");

            migrationBuilder.RenameColumn(
                name: "AwardType",
                table: "Awards",
                newName: "AwardTypeId");

            migrationBuilder.CreateTable(
                name: "AwardTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AwardTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsJournal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Publications_PublicationTypeId",
                table: "Publications",
                column: "PublicationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeId",
                table: "Events",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_AwardTypeId",
                table: "Awards",
                column: "AwardTypeId");

            // ── Seed lookup tables before FK constraints ─────────────────────────────────────

            migrationBuilder.Sql(@"
                INSERT INTO ""AwardTypes"" (""Id"", ""Name"") VALUES
                (0, 'Premio de la Academia de Ciencias'),
                (1, 'Premio MES'),
                (2, 'Premio CITMA Innovación Tecnológica'),
                (3, 'Premio CITMA Estudiantes y Jóvenes Investigadores'),
                (4, 'Premio Forum Ciencia y Técnica'),
                (5, 'Premio Investigación UH'),
                (6, 'Otros premios (prensa, salud, sociedad, etc.)'),
                (7, 'Premio Internacional');
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ""EventTypes"" (""Id"", ""Name"") VALUES
                (0, 'Internacional'),
                (1, 'Nacional'),
                (2, 'Regional'),
                (3, 'Local');
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ""PublicationTypes"" (""Id"", ""Name"", ""IsJournal"") VALUES
                (0, 'Diario',                   true),
                (1, 'Libro',                    false),
                (2, 'Monografía',               false),
                (3, 'Capítulo',                 false),
                (4, 'Artículo de Divulgación',  false);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_AwardTypes_AwardTypeId",
                table: "Awards",
                column: "AwardTypeId",
                principalTable: "AwardTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventTypes_EventTypeId",
                table: "Events",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_PublicationTypes_PublicationTypeId",
                table: "Publications",
                column: "PublicationTypeId",
                principalTable: "PublicationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Awards_AwardTypes_AwardTypeId",
                table: "Awards");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventTypes_EventTypeId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Publications_PublicationTypes_PublicationTypeId",
                table: "Publications");

            migrationBuilder.DropTable(
                name: "AwardTypes");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "PublicationTypes");

            migrationBuilder.DropIndex(
                name: "IX_Publications_PublicationTypeId",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventTypeId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Awards_AwardTypeId",
                table: "Awards");

            migrationBuilder.RenameColumn(
                name: "PublicationTypeId",
                table: "Publications",
                newName: "PublicationType");

            migrationBuilder.RenameColumn(
                name: "EventTypeId",
                table: "Events",
                newName: "EventType");

            migrationBuilder.RenameColumn(
                name: "AwardTypeId",
                table: "Awards",
                newName: "AwardType");
        }
    }
}
