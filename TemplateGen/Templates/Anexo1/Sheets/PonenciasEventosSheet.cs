using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class PonenciasEventosSheet : ISheetTemplate
{
    public string Name         => "Ponencias en Eventos";
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

        Anexo1SheetHelper.WriteTitle(ws, 1, "Tabla 9. Ponencias en eventos científicos (resumen)", 7);

        // Row 2: top-level headers — Tipo + merged Nacionales + merged Internacionales + merged Total
        Anexo1SheetHelper.WriteMergedHeader(ws, 2, 1, 1, "Tipo");
        Anexo1SheetHelper.WriteMergedHeader(ws, 2, 2, 3, "Eventos Nacionales");
        Anexo1SheetHelper.WriteMergedHeader(ws, 2, 4, 5, "Eventos Internacionales");
        Anexo1SheetHelper.WriteMergedHeader(ws, 2, 6, 7, "Total");

        // Row 3: sub-headers Plan / Real under each group
        Anexo1SheetHelper.WriteHeaderRow(ws, 3, string.Empty, "Plan", "Real", "Plan", "Real", "Plan", "Real");
        ws.Row(3).Height = 28;

        // Data rows
        Anexo1SheetHelper.WriteLabel(ws, 4, 1, "Ponencias");
        Anexo1SheetHelper.WriteBlank(ws, 4, 2);
        Anexo1SheetHelper.WriteScalar(ws, 4, 3, "PonenciasEventosNacionalesReal");
        Anexo1SheetHelper.WriteBlank(ws, 4, 4);
        Anexo1SheetHelper.WriteScalar(ws, 4, 5, "PonenciasEventosInternacionalesReal");
        Anexo1SheetHelper.WriteBlank(ws, 4, 6);
        Anexo1SheetHelper.WriteScalar(ws, 4, 7, "PonenciasEventosTotalReal");

        // Índice por profesor
        Anexo1SheetHelper.WriteTitle(ws, 6, "Indicadores derivados", 7);
        Anexo1SheetHelper.WriteLabel(ws, 7, 1, "Ponencias por profesor");
        Anexo1SheetHelper.WriteScalar(ws, 7, 2, "PonenciasPorProfesor");
        for (int c = 3; c <= 7; c++) Anexo1SheetHelper.WriteBlank(ws, 7, c);
    }
}
