using ClosedXML.Excel;
using TemplateGen.Core.Interfaces;

namespace TemplateGen.Templates.Anexo1;

public sealed class PatentesRegistrosSheet : ISheetTemplate
{
    public string Name         => "Patentes y Registros";
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

        Anexo1SheetHelper.WriteTitle(ws, 1, "Tabla 19. Protección a la propiedad intelectual (resumen)", 4);
        Anexo1SheetHelper.WriteHeaderRow(ws, 2, "Tipo", "Cuba / Informáticas", "Extranjero / No Informáticas", "Total");

        // Patentes
        Anexo1SheetHelper.WriteDataRow(ws, 3, "Patentes", 4, [
            (2, "PatentesCuba"),
            (3, "PatentesExtranjero"),
            (4, "PatentesTotal"),
        ]);

        // Registros
        Anexo1SheetHelper.WriteDataRow(ws, 4, "Registros de Software", 4, [
            (2, "RegistrosInformaticos"),
            (3, "RegistrosNoInformaticos"),
            (4, "RegistrosTotal"),
        ]);

        // Normas
        Anexo1SheetHelper.WriteTitle(ws, 5, "Normas por tipo", 4);
        Anexo1SheetHelper.WriteLabel(ws, 6, 1, "Nacionales");
        Anexo1SheetHelper.WriteScalar(ws, 6, 2, "NormasNacionales");
        Anexo1SheetHelper.WriteBlank(ws, 6, 3);
        Anexo1SheetHelper.WriteBlank(ws, 6, 4);

        Anexo1SheetHelper.WriteLabel(ws, 7, 1, "Ramales");
        Anexo1SheetHelper.WriteScalar(ws, 7, 2, "NormasRamales");
        Anexo1SheetHelper.WriteBlank(ws, 7, 3);
        Anexo1SheetHelper.WriteBlank(ws, 7, 4);

        Anexo1SheetHelper.WriteLabel(ws, 8, 1, "Empresariales");
        Anexo1SheetHelper.WriteScalar(ws, 8, 2, "NormasEmpresariales");
        Anexo1SheetHelper.WriteBlank(ws, 8, 3);
        Anexo1SheetHelper.WriteBlank(ws, 8, 4);

        Anexo1SheetHelper.WriteLabel(ws, 9, 1, "Total Normas", bold: true);
        Anexo1SheetHelper.WriteScalar(ws, 9, 2, "NormasTotal");
        Anexo1SheetHelper.WriteBlank(ws, 9, 3);
        Anexo1SheetHelper.WriteBlank(ws, 9, 4);

        // Marcas (not modeled — blank)
        Anexo1SheetHelper.WriteDataRow(ws, 10, "Marcas comerciales", 4);
    }
}
