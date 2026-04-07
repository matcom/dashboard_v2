using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashboard_v2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaDelConocimientoAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AreasDelConocimiento",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreasDelConocimiento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AreasInvestiganAreasDelConocimiento",
                columns: table => new
                {
                    AreaId = table.Column<string>(type: "character varying(450)", nullable: false),
                    AreaDelConocimientoId = table.Column<string>(type: "character varying(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreasInvestiganAreasDelConocimiento", x => new { x.AreaId, x.AreaDelConocimientoId });
                    table.ForeignKey(
                        name: "FK_AreasInvestiganAreasDelConocimiento_AreasDelConocimiento_Ar~",
                        column: x => x.AreaDelConocimientoId,
                        principalTable: "AreasDelConocimiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AreasInvestiganAreasDelConocimiento_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineasDeInvestigacion",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    AreaDelConocimientoId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasDeInvestigacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasDeInvestigacion_AreasDelConocimiento_AreaDelConocimie~",
                        column: x => x.AreaDelConocimientoId,
                        principalTable: "AreasDelConocimiento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GruposDeInvestigacionLineasDeInvestigacion",
                columns: table => new
                {
                    GrupoDeInvestigacionId = table.Column<string>(type: "character varying(450)", nullable: false),
                    LineaDeInvestigacionId = table.Column<string>(type: "character varying(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposDeInvestigacionLineasDeInvestigacion", x => new { x.GrupoDeInvestigacionId, x.LineaDeInvestigacionId });
                    table.ForeignKey(
                        name: "FK_GruposDeInvestigacionLineasDeInvestigacion_GruposDeInvestig~",
                        column: x => x.GrupoDeInvestigacionId,
                        principalTable: "GruposDeInvestigacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GruposDeInvestigacionLineasDeInvestigacion_LineasDeInvestig~",
                        column: x => x.LineaDeInvestigacionId,
                        principalTable: "LineasDeInvestigacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AreasInvestiganAreasDelConocimiento_AreaDelConocimientoId",
                table: "AreasInvestiganAreasDelConocimiento",
                column: "AreaDelConocimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposDeInvestigacionLineasDeInvestigacion_LineaDeInvestiga~",
                table: "GruposDeInvestigacionLineasDeInvestigacion",
                column: "LineaDeInvestigacionId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasDeInvestigacion_AreaDelConocimientoId",
                table: "LineasDeInvestigacion",
                column: "AreaDelConocimientoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreasInvestiganAreasDelConocimiento");

            migrationBuilder.DropTable(
                name: "GruposDeInvestigacionLineasDeInvestigacion");

            migrationBuilder.DropTable(
                name: "LineasDeInvestigacion");

            migrationBuilder.DropTable(
                name: "AreasDelConocimiento");
        }
    }
}
