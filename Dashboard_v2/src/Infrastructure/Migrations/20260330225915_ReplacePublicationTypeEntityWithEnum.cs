using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePublicationTypeEntityWithEnum : Migration
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

            migrationBuilder.DropColumn(
                name: "PublicationTypeId",
                table: "Publications");

            migrationBuilder.AddColumn<int>(
                name: "PublicationType",
                table: "Publications",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicationType",
                table: "Publications");

            migrationBuilder.AddColumn<string>(
                name: "PublicationTypeId",
                table: "Publications",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PublicationTypes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_PublicationTypes_Name",
                table: "PublicationTypes",
                column: "Name",
                unique: true);

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
