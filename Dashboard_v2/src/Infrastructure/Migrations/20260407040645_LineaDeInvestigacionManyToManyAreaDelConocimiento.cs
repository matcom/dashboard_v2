using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LineaDeInvestigacionManyToManyAreaDelConocimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LineasDeInvestigacion_AreasDelConocimiento_AreaDelConocimie~",
                table: "LineasDeInvestigacion");

            migrationBuilder.DropIndex(
                name: "IX_LineasDeInvestigacion_AreaDelConocimientoId",
                table: "LineasDeInvestigacion");

            migrationBuilder.DropColumn(
                name: "AreaDelConocimientoId",
                table: "LineasDeInvestigacion");

            migrationBuilder.CreateTable(
                name: "AreasDelConocimientoLineasDeInvestigacion",
                columns: table => new
                {
                    AreaDelConocimientoId = table.Column<string>(type: "character varying(450)", nullable: false),
                    LineaDeInvestigacionId = table.Column<string>(type: "character varying(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreasDelConocimientoLineasDeInvestigacion", x => new { x.AreaDelConocimientoId, x.LineaDeInvestigacionId });
                    table.ForeignKey(
                        name: "FK_AreasDelConocimientoLineasDeInvestigacion_AreasDelConocimie~",
                        column: x => x.AreaDelConocimientoId,
                        principalTable: "AreasDelConocimiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AreasDelConocimientoLineasDeInvestigacion_LineasDeInvestiga~",
                        column: x => x.LineaDeInvestigacionId,
                        principalTable: "LineasDeInvestigacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AreasDelConocimientoLineasDeInvestigacion_LineaDeInvestigac~",
                table: "AreasDelConocimientoLineasDeInvestigacion",
                column: "LineaDeInvestigacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreasDelConocimientoLineasDeInvestigacion");

            migrationBuilder.AddColumn<string>(
                name: "AreaDelConocimientoId",
                table: "LineasDeInvestigacion",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LineasDeInvestigacion_AreaDelConocimientoId",
                table: "LineasDeInvestigacion",
                column: "AreaDelConocimientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_LineasDeInvestigacion_AreasDelConocimiento_AreaDelConocimie~",
                table: "LineasDeInvestigacion",
                column: "AreaDelConocimientoId",
                principalTable: "AreasDelConocimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
