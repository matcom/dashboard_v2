using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class ProyectosDLSheet : ISheetTemplate
{
    public string Name         => "Proyectos DL";
    public string Title        => string.Empty;
    public string[] Headers    => [];
    public string RangeName    => string.Empty;
    public int StartRow        => 0;
    public int StartCol        => 1;
    public int EndRowOffset    => 0;
    public IEnumerable<(int Col, string Expression)> TemplateCells => [];

    public void Generate(IXLWorkbook wb)
    {
        var ws = wb.Worksheets.Add(Name);
        Anexo1SheetHelper.SetWidths(ws, 36, 20, 20, 20, 20, 20);

        Anexo1SheetHelper.WriteTitle(ws, 1, "Proyectos de Desarrollo Local (resumen por tipo y estado)", 6);
        Anexo1SheetHelper.WriteHeaderRow(ws, 2, "Tipo / Indicador", "Total", "Terminados", "En Ejecución", "Atrasados", "Cancelados");

        // 5 sub-type rows — not modeled in domain, all blank
        var subtipos = new[]
        {
            "Económico-productivos",
            "Socioculturales",
            "Medioambientales",
            "Científico-tecnológicos",
            "Otros",
        };
        int row = 3;
        foreach (var tipo in subtipos)
        {
            Anexo1SheetHelper.WriteDataRow(ws, row, tipo, 6);
            row++;
        }

        // TOTAL row — auto-filled from domain
        Anexo1SheetHelper.WriteLabel(ws, row, 1, "TOTAL", bold: true);
        Anexo1SheetHelper.WriteScalar(ws, row, 2, "PDLTotal");
        Anexo1SheetHelper.WriteScalar(ws, row, 3, "PDLTerminados");
        Anexo1SheetHelper.WriteScalar(ws, row, 4, "PDLEnEjecucion");
        Anexo1SheetHelper.WriteScalar(ws, row, 5, "PDLAtrasados");
        Anexo1SheetHelper.WriteScalar(ws, row, 6, "PDLCancelados");
        row++;

        // Additional indicators
        Anexo1SheetHelper.WriteTitle(ws, row, "Indicadores de contribución", 6);
        row++;

        Anexo1SheetHelper.WriteLabel(ws, row, 1, "Contribución Total (PDL)");
        Anexo1SheetHelper.WriteScalar(ws, row, 2, "PDLContribucionTotal");
        for (int c = 3; c <= 6; c++) Anexo1SheetHelper.WriteBlank(ws, row, c);
        row++;

        Anexo1SheetHelper.WriteLabel(ws, row, 1, "Monto económico total (CUP)");
        for (int c = 2; c <= 6; c++) Anexo1SheetHelper.WriteBlank(ws, row, c);
        row++;

        // Note
        var noteCell = ws.Cell(row + 1, 1);
        noteCell.Value = "Nota: Los sub-tipos de PDL no están modelados en el sistema. Complete las filas de tipo manualmente.";
        noteCell.Style.Font.Italic = true;
        noteCell.Style.Font.FontColor = XLColor.DarkGray;
        ws.Range(row + 1, 1, row + 1, 6).Merge();
    }
}
