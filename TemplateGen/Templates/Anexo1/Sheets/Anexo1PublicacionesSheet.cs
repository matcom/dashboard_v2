using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class Anexo1PublicacionesSheet : ISheetTemplate
{
    public string Name         => "Publicaciones Resumen";
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
        Anexo1SheetHelper.SetWidths(ws, 36, 22, 22);

        Anexo1SheetHelper.WriteTitle(ws, 1, "Tabla 7 (parcial). Publicaciones por tipo y grupo (resumen)", 3);
        Anexo1SheetHelper.WriteHeaderRow(ws, 2, "Tipo de Publicación", "Plan", "Real");

        var rows = new (string Label, string? VarName)[]
        {
            ("Artículos G1 (WoS/Scopus Q1)",           "G1Count"),
            ("Artículos G2 (WoS/Scopus Q2–Q4 / SCI)",  "G2Count"),
            ("Artículos G3 (Scielo/DOAJ/MEDLINE)",      "G3Count"),
            ("Artículos de Divulgación",                "ArticulosDivulgacionCount"),
            ("Libros y capítulos de libro",             null),
            ("Tesis de doctorado",                     null),
            ("Tesis de maestría",                      null),
        };

        int row = 3;
        foreach (var (label, varName) in rows)
        {
            Anexo1SheetHelper.WriteLabel(ws, row, 1, label);
            Anexo1SheetHelper.WriteBlank(ws, row, 2);
            if (varName is not null)
                Anexo1SheetHelper.WriteScalar(ws, row, 3, varName);
            else
                Anexo1SheetHelper.WriteBlank(ws, row, 3);
            row++;
        }

        // Note
        var noteCell = ws.Cell(row + 1, 1);
        noteCell.Value = "Nota: Solo G1, G2, G3 y Divulgación son calculados automáticamente. Los demás tipos se completan manualmente.";
        noteCell.Style.Font.Italic = true;
        noteCell.Style.Font.FontColor = XLColor.DarkGray;
        ws.Range(row + 1, 1, row + 1, 3).Merge();
    }
}
