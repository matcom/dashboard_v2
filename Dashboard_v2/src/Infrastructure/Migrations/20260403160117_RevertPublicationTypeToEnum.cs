using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RevertPublicationTypeToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_PublicationTypes_PublicationTypeId",
                table: "Publications");

            migrationBuilder.DropTable(
                name: "PublicationTypes");

            migrationBuilder.DropIndex(
                name: "IX_Publications_PublicationTypeId",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "PublicationTypeId",
                table: "Publications",
                newName: "PublicationType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublicationType",
                table: "Publications",
                newName: "PublicationTypeId");

            migrationBuilder.CreateTable(
                name: "PublicationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsJournal = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Publications_PublicationTypeId",
                table: "Publications",
                column: "PublicationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_PublicationTypes_PublicationTypeId",
                table: "Publications",
                column: "PublicationTypeId",
                principalTable: "PublicationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
