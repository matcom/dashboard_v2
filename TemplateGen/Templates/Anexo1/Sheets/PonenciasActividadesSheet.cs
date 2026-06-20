using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

/// <summary>
/// Ponencias en actividades científicas internas (UH).
/// No están modeladas en el dominio — todas las celdas de valor quedan en blanco
/// para rellenar manualmente. Se incluye una nota al pie.
/// </summary>
public sealed class PonenciasActividadesSheet : ISheetTemplate
{
    public string Name         => "Ponencias Actividades";
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
        Anexo1SheetHelper.SetWidths(ws, 28, 22, 22, 22, 22, 22, 22);

        Anexo1SheetHelper.WriteTitle(ws, 1, "Tabla 9b. Ponencias en actividades científicas internas (resumen)", 7);

        Anexo1SheetHelper.WriteMergedHeader(ws, 2, 1, 1, "Tipo");
        Anexo1SheetHelper.WriteMergedHeader(ws, 2, 2, 3, "Fórum de Ciencia y Técnica");
        Anexo1SheetHelper.WriteMergedHeader(ws, 2, 4, 5, "Jornadas Científicas");
        Anexo1SheetHelper.WriteMergedHeader(ws, 2, 6, 7, "Total");

        Anexo1SheetHelper.WriteHeaderRow(ws, 3, string.Empty, "Plan", "Real", "Plan", "Real", "Plan", "Real");
        ws.Row(3).Height = 28;

        // All values blank — not modeled in domain
        Anexo1SheetHelper.WriteLabel(ws, 4, 1, "Ponencias");
        for (int c = 2; c <= 7; c++) Anexo1SheetHelper.WriteBlank(ws, 4, c);

        // Note
        var noteCell = ws.Cell(6, 1);
        noteCell.Value = "Nota: Las actividades científicas internas (Fórum, Jornadas) no están modeladas en el sistema. Complete manualmente.";
        noteCell.Style.Font.Italic = true;
        noteCell.Style.Font.FontColor = XLColor.DarkGray;
        ws.Range(6, 1, 6, 7).Merge();
    }
}
