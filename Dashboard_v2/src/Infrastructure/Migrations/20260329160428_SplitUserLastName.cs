using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitUserLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserLastName",
                table: "Users",
                newName: "UserLastName1");

            migrationBuilder.AddColumn<string>(
                name: "UserLastName2",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLastName2",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserLastName1",
                table: "Users",
                newName: "UserLastName");
        }
    }
}
