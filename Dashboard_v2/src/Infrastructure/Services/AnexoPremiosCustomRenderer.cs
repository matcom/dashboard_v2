using ClosedXML.Excel;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Documents.Reports;

namespace Dashboard_v2.Infrastructure.Services;

public sealed class AnexoPremiosCustomRenderer : ICustomDocumentRenderer
{
    public string TemplateName => "AnexoPremios";

    public byte[] Render(Stream templateStream, IReadOnlyDictionary<string, object> variables)
    {
        using var workbook = new XLWorkbook(templateStream);
        var ws = workbook.Worksheet(1);

        var tiposPremio = variables.TryGetValue("TiposPremio", out var rawValue)
            ? rawValue as IEnumerable<AnexoPremiosTipoRowDto>
            : Enumerable.Empty<AnexoPremiosTipoRowDto>();

        const int firstBodyRow = 4;
        const int typePrototypeRow = 4;
        const int headerPrototypeRow = 5;
        const int awardPrototypeRow = 6;
        const int lastColumn = 3;
        const int existingPrototypeRows = 3;

        var totalRowsNeeded = tiposPremio?.Sum(type => 2 + Math.Max(1, type.Premios.Count)) ?? 0;
        var bodyRowCount = Math.Max(existingPrototypeRows, totalRowsNeeded);

        if (totalRowsNeeded > existingPrototypeRows)
            ws.Row(awardPrototypeRow).InsertRowsBelow(totalRowsNeeded - existingPrototypeRows);

        var currentRow = firstBodyRow;
        foreach (var tipo in tiposPremio ?? Enumerable.Empty<AnexoPremiosTipoRowDto>())
        {
            ExcelRenderHelpers.CopyRowLayout(ws, typePrototypeRow, currentRow, lastColumn);
            ws.Cell(currentRow, 1).Value = tipo.Numero;
            ws.Cell(currentRow, 2).Value = tipo.TipoPremio;
            ws.Cell(currentRow, 3).Value = string.Empty;
            currentRow++;

            ExcelRenderHelpers.CopyRowLayout(ws, headerPrototypeRow, currentRow, lastColumn);
            ws.Cell(currentRow, 1).Value = string.Empty;
            ws.Cell(currentRow, 2).Value = "Titulo";
            ws.Cell(currentRow, 3).Value = "Autores";
            currentRow++;

            if (tipo.Premios.Count > 0)
            {
                foreach (var premio in tipo.Premios)
                {
                    ExcelRenderHelpers.CopyRowLayout(ws, awardPrototypeRow, currentRow, lastColumn);
                    ws.Cell(currentRow, 1).Value = string.Empty;
                    ws.Cell(currentRow, 2).Value = premio.Titulo;
                    ws.Cell(currentRow, 3).Value = premio.Autores;
                    currentRow++;
                }
            }
            else
            {
                ExcelRenderHelpers.CopyRowLayout(ws, awardPrototypeRow, currentRow, lastColumn);
                currentRow++;
            }
        }

        ExcelRenderHelpers.ClearUnusedRows(ws, currentRow, firstBodyRow + bodyRowCount - 1, lastColumn);

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }
}
