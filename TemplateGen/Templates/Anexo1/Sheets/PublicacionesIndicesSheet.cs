using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class PublicacionesIndicesSheet : ISheetTemplate
{
    public string Name         => "Publicaciones Índices";
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
        Anexo1SheetHelper.SetWidths(ws, 40, 26, 26);

        Anexo1SheetHelper.WriteTitle(ws, 1, "Indicadores de publicaciones por profesor y por doctor", 3);
        Anexo1SheetHelper.WriteHeaderRow(ws, 2, "Indicador", "Por Profesor", "Por Doctor");

        var rows = new (string Label, string? ByProf, string? ByDoc)[]
        {
            ("Índice total de publicaciones",                "IndicePublicacionesTotalProfesor",  "IndicePublicacionesTotalDoctor"),
            ("Índice publicaciones WoS/Scopus (G1+G2)",     "IndicePublicacionesWosProfesor",    "IndicePublicacionesWosDoctor"),
            ("Artículos G2 (valor absoluto)",               "IndiceArticulosG2",                 null),
        };

        int row = 3;
        foreach (var (label, byProf, byDoc) in rows)
        {
            Anexo1SheetHelper.WriteLabel(ws, row, 1, label);
            if (byProf is not null)
                Anexo1SheetHelper.WriteScalar(ws, row, 2, byProf);
            else
                Anexo1SheetHelper.WriteBlank(ws, row, 2);
            if (byDoc is not null)
                Anexo1SheetHelper.WriteScalar(ws, row, 3, byDoc);
            else
                Anexo1SheetHelper.WriteBlank(ws, row, 3);
            row++;
        }

        // Note
        var noteCell = ws.Cell(row + 1, 1);
        noteCell.Value = "Nota: Los índices por doctor no están calculados — el sistema no modela grados académicos de profesores. Complete manualmente.";
        noteCell.Style.Font.Italic = true;
        noteCell.Style.Font.FontColor = XLColor.DarkGray;
        ws.Range(row + 1, 1, row + 1, 3).Merge();
    }
}
