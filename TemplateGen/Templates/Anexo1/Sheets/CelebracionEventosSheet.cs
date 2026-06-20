using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class CelebracionEventosSheet : ISheetTemplate
{
    public string Name         => "Celebración de Eventos";
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
        Anexo1SheetHelper.SetWidths(ws, 30, 22, 22, 22);

        Anexo1SheetHelper.WriteTitle(ws, 1, "Tabla 10. Celebración de eventos científicos (resumen)", 4);
        Anexo1SheetHelper.WriteHeaderRow(ws, 2, "Tipo de Evento", "Plan", "Real", "Observaciones");

        // Organizados (auto-filled)
        Anexo1SheetHelper.WriteLabel(ws, 3, 1, "Eventos Organizados");
        Anexo1SheetHelper.WriteBlank(ws, 3, 2);
        Anexo1SheetHelper.WriteScalar(ws, 3, 3, "EventosOrganizadosReal");
        Anexo1SheetHelper.WriteBlank(ws, 3, 4);

        // Coauspiciados (auto-filled)
        Anexo1SheetHelper.WriteLabel(ws, 4, 1, "Eventos Coauspiciados");
        Anexo1SheetHelper.WriteBlank(ws, 4, 2);
        Anexo1SheetHelper.WriteScalar(ws, 4, 3, "EventosCoauspiciadosReal");
        Anexo1SheetHelper.WriteBlank(ws, 4, 4);

        // Actividades internas (not modeled — blank)
        Anexo1SheetHelper.WriteLabel(ws, 5, 1, "Actividades Científicas Internas");
        Anexo1SheetHelper.WriteBlank(ws, 5, 2);
        Anexo1SheetHelper.WriteBlank(ws, 5, 3);
        var obsCell = ws.Cell(5, 4);
        obsCell.Value = "No modelado — completar manualmente";
        obsCell.Style.Font.Italic = true;
        obsCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Note
        var noteCell = ws.Cell(7, 1);
        noteCell.Value = "Nota: Las actividades científicas internas (Fórum, Jornadas) no están modeladas en el sistema.";
        noteCell.Style.Font.Italic = true;
        noteCell.Style.Font.FontColor = XLColor.DarkGray;
        ws.Range(7, 1, 7, 4).Merge();
    }
}
