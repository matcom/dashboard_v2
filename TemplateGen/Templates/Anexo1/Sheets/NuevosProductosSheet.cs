using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class NuevosProductosSheet : ISheetTemplate
{
    public string Name         => "Nuevos Productos";
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
        Anexo1SheetHelper.SetWidths(ws, 34, 22, 22);

        Anexo1SheetHelper.WriteTitle(ws, 1, "Tabla 18. Nuevos productos, tecnologías y servicios creados (resumen)", 3);
        Anexo1SheetHelper.WriteHeaderRow(ws, 2, "Tipo", "Plan", "Real");

        var rows = new[]
        {
            ("Nuevos Productos",           "NuevosProductos"),
            ("Nuevas Tecnologías",         "NuevasTecnologias"),
            ("Nuevos Servicios",           "NuevosServicios"),
            ("Total",                      "NuevosProductosTecServTotal"),
        };

        int row = 3;
        foreach (var (label, varName) in rows)
        {
            bool isTotal = row == 6;
            Anexo1SheetHelper.WriteLabel(ws, row, 1, label, bold: isTotal);
            Anexo1SheetHelper.WriteBlank(ws, row, 2);
            Anexo1SheetHelper.WriteScalar(ws, row, 3, varName);
            row++;
        }
    }
}
