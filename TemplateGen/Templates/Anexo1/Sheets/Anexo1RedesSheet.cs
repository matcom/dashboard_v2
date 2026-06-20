using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class Anexo1RedesSheet : ISheetTemplate
{
    public string Name         => "Redes";
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
        Anexo1SheetHelper.SetWidths(ws, 22, 40, 24, 24);

        Anexo1SheetHelper.WriteTitle(ws, 1, "Tabla 5. Participación en redes científicas (resumen por tipo)", 4);
        Anexo1SheetHelper.WriteHeaderRow(ws, 2, "Tipo de Red", "Nombre de cada red", "Profesores participantes", "Estudiantes participantes");

        var rows = new[]
        {
            ("Universitarias",  "RedesUniversitariasProfesores",  "RedesUniversitariasEstudiantes"),
            ("Nacionales",      "RedesNacionalesProfesores",      "RedesNacionalesEstudiantes"),
            ("Internacionales", "RedesInternacionalesProfesores", "RedesInternacionalesEstudiantes"),
            ("Total",           "RedesTotalProfesores",           "RedesTotalEstudiantes"),
        };

        int row = 3;
        foreach (var (tipo, profVar, estVar) in rows)
        {
            Anexo1SheetHelper.WriteLabel(ws, row, 1, tipo);
            Anexo1SheetHelper.WriteBlank(ws, row, 2);
            Anexo1SheetHelper.WriteScalar(ws, row, 3, profVar);
            Anexo1SheetHelper.WriteScalar(ws, row, 4, estVar);
            row++;
        }
    }
}
