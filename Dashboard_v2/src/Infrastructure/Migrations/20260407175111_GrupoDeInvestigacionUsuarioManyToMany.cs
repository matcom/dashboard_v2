using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GrupoDeInvestigacionUsuarioManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GruposDeInvestigacionUsuarios",
                columns: table => new
                {
                    GrupoDeInvestigacionId = table.Column<string>(type: "character varying(450)", nullable: false),
                    UsuarioId = table.Column<string>(type: "character varying(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposDeInvestigacionUsuarios", x => new { x.GrupoDeInvestigacionId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_GruposDeInvestigacionUsuarios_GruposDeInvestigacion_GrupoDe~",
                        column: x => x.GrupoDeInvestigacionId,
                        principalTable: "GruposDeInvestigacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GruposDeInvestigacionUsuarios_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GruposDeInvestigacionUsuarios_UsuarioId",
                table: "GruposDeInvestigacionUsuarios",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GruposDeInvestigacionUsuarios");
        }
    }
}
