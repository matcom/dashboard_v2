using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class Anexo1PremiosSheet : ISheetTemplate
{
    public string Name         => "Premios";
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

        Anexo1SheetHelper.WriteTitle(ws, 1, "Tabla 14. Premios obtenidos por tipo (resumen)", 3);
        Anexo1SheetHelper.WriteHeaderRow(ws, 2, "Tipo de Premio", "Plan (compromiso)", "Real (cumplido)");

        // List prototype + Named Range "Premios" (cols 1-3, rows 3-4)
        Anexo1SheetHelper.WriteListRange(ws, "Premios", 3, 3,
        [
            (1, "{{item.TipoPremio}}"),
            (3, "{{item.Cantidad}}"),
        ]);
    }
}
