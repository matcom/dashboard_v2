using ClosedXML.Excel;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Documents.Reports;

namespace Dashboard_v2.Infrastructure.Services;

public sealed class AnexoRegistrosCustomRenderer : ICustomDocumentRenderer
{
    public string TemplateName => "AnexoRegistros";

    public byte[] Render(Stream templateStream, IReadOnlyDictionary<string, object> variables)
    {
        using var workbook = new XLWorkbook(templateStream);

        // --- Patentes ---
        var patentes = variables.TryGetValue("Patentes", out var rawPat) ? rawPat as IEnumerable<AnexoRegistrosPatenteRowDto> : Enumerable.Empty<AnexoRegistrosPatenteRowDto>();
        var wsPat = workbook.Worksheet("Patentes");
        const int patFirstBodyRow = 5;
        const int patDataPrototypeRow = 5;
        const int patLastColumn = 4;
        const int patExistingPrototypeRows = 1;

        var patTotalRowsNeeded = patentes?.Count() ?? 0;
        if (patTotalRowsNeeded > patExistingPrototypeRows)
            wsPat.Row(patDataPrototypeRow).InsertRowsBelow(patTotalRowsNeeded - patExistingPrototypeRows);

        var currentRowPat = patFirstBodyRow;
        if (patentes != null && patentes.Any())
        {
            foreach (var p in patentes)
            {
                ExcelRenderHelpers.CopyRowLayout(wsPat, patDataPrototypeRow, currentRowPat, patLastColumn);
                wsPat.Cell(currentRowPat, 1).Value = p.Titulo;
                wsPat.Cell(currentRowPat, 2).Value = p.NumeroSolicitudConcesion;
                wsPat.Cell(currentRowPat, 3).Value = p.EsNacional ? "X" : string.Empty;
                wsPat.Cell(currentRowPat, 4).Value = p.EsNacional ? string.Empty : "X";
                currentRowPat++;
            }
        }
        else
        {
            currentRowPat++;
        }

        ExcelRenderHelpers.ClearUnusedRows(wsPat, currentRowPat, patFirstBodyRow + Math.Max(patExistingPrototypeRows, patTotalRowsNeeded) - 1, patLastColumn);

        // --- Registros (two tables on same sheet) ---
        var registrosInfo = variables.TryGetValue("RegistrosInformaticos", out var rawRI) ? rawRI as IEnumerable<AnexoRegistroRowDto> : Enumerable.Empty<AnexoRegistroRowDto>();
        var registrosNoInfo = variables.TryGetValue("RegistrosNoInformaticos", out var rawRNI) ? rawRNI as IEnumerable<AnexoRegistroRowDto> : Enumerable.Empty<AnexoRegistroRowDto>();
        var wsReg = workbook.Worksheet("Registros");

        const int regPrototype1 = 5;
        const int regHeader2Base = 8;
        const int regDataOffset = 1;
        const int regLastCol = 4;
        const int regExistingProto = 1;

        var reg1Count = registrosInfo?.Count() ?? 0;
        if (reg1Count > regExistingProto)
            wsReg.Row(regPrototype1).InsertRowsBelow(reg1Count - regExistingProto);

        var extraRowsFirst = Math.Max(0, reg1Count - regExistingProto);
        var header2Row = regHeader2Base + extraRowsFirst;
        var dataPrototype2 = header2Row + regDataOffset;

        var reg2Count = registrosNoInfo?.Count() ?? 0;
        if (reg2Count > regExistingProto)
            wsReg.Row(dataPrototype2).InsertRowsBelow(reg2Count - regExistingProto);

        var currentRow1 = regPrototype1;
        if (registrosInfo != null && registrosInfo.Any())
        {
            foreach (var r in registrosInfo)
            {
                ExcelRenderHelpers.CopyRowLayout(wsReg, regPrototype1, currentRow1, regLastCol);
                wsReg.Cell(currentRow1, 1).Value = r.Titulo;
                wsReg.Cell(currentRow1, 2).Value = r.InstitutionNombre;
                wsReg.Cell(currentRow1, 3).Value = r.NumeroCertificado;
                wsReg.Cell(currentRow1, 4).Value = r.CountryName;
                currentRow1++;
            }
        }
        else currentRow1++;

        var reg1BodyCount = Math.Max(regExistingProto, reg1Count);
        ExcelRenderHelpers.ClearUnusedRows(wsReg, currentRow1, regPrototype1 + reg1BodyCount - 1, regLastCol);

        var currentRow2 = dataPrototype2;
        if (registrosNoInfo != null && registrosNoInfo.Any())
        {
            foreach (var r in registrosNoInfo)
            {
                ExcelRenderHelpers.CopyRowLayout(wsReg, dataPrototype2, currentRow2, regLastCol);
                wsReg.Cell(currentRow2, 1).Value = r.Titulo;
                wsReg.Cell(currentRow2, 2).Value = r.InstitutionNombre;
                wsReg.Cell(currentRow2, 3).Value = r.NumeroCertificado;
                wsReg.Cell(currentRow2, 4).Value = r.CountryName;
                currentRow2++;
            }
        }
        else currentRow2++;

        var reg2BodyCount = Math.Max(regExistingProto, reg2Count);
        ExcelRenderHelpers.ClearUnusedRows(wsReg, currentRow2, dataPrototype2 + reg2BodyCount - 1, regLastCol);

        // --- Normas ---
        var normas = variables.TryGetValue("Normas", out var rawNormas) ? rawNormas as IEnumerable<AnexoNormaRowDto> : Enumerable.Empty<AnexoNormaRowDto>();
        var wsNormas = workbook.Worksheet("Normas");
        const int normasPrototypeRow = 5;
        const int normasLastCol = 3;
        const int normasExistingProto = 1;
        var normasCount = normas?.Count() ?? 0;
        if (normasCount > normasExistingProto) wsNormas.Row(normasPrototypeRow).InsertRowsBelow(normasCount - normasExistingProto);

        var curNormRow = normasPrototypeRow;
        if (normas != null && normas.Any())
        {
            foreach (var n in normas)
            {
                ExcelRenderHelpers.CopyRowLayout(wsNormas, normasPrototypeRow, curNormRow, normasLastCol);
                wsNormas.Cell(curNormRow, 1).Value = n.Titulo;
                wsNormas.Cell(curNormRow, 2).Value = n.Tipo;
                wsNormas.Cell(curNormRow, 3).Value = n.InstitutionNombre;
                curNormRow++;
            }
        }
        else curNormRow++;

        ExcelRenderHelpers.ClearUnusedRows(wsNormas, curNormRow, normasPrototypeRow + Math.Max(normasExistingProto, normasCount) - 1, normasLastCol);

        // --- Productos Comercializados (agrupado por tipo) ---
        var tiposProductos = variables.TryGetValue("ProductosTipos", out var rawTipos) ? rawTipos as IEnumerable<AnexoProductoTipoRowDto> : Enumerable.Empty<AnexoProductoTipoRowDto>();
        var wsProd = workbook.Worksheet("ProductosComercializados");
        const int prodFirstBodyRow = 4;
        const int prodTypePrototypeRow = 4;
        const int prodHeaderPrototypeRow = 5;
        const int prodProductPrototypeRow = 6;
        const int prodLastColumn = 3;
        const int prodExistingPrototypeRows = 3;

        var totalRowsNeededProd = tiposProductos?.Sum(t => 2 + Math.Max(1, t.Productos.Count)) ?? 0;
        var bodyRowCountProd = Math.Max(prodExistingPrototypeRows, totalRowsNeededProd);

        if (totalRowsNeededProd > prodExistingPrototypeRows)
            wsProd.Row(prodProductPrototypeRow).InsertRowsBelow(totalRowsNeededProd - prodExistingPrototypeRows);

        var currProdRow = prodFirstBodyRow;
        foreach (var tipo in tiposProductos ?? Enumerable.Empty<AnexoProductoTipoRowDto>())
        {
            ExcelRenderHelpers.CopyRowLayout(wsProd, prodTypePrototypeRow, currProdRow, prodLastColumn);
            wsProd.Cell(currProdRow, 1).Value = tipo.TipoProductoComercializadoNombre;
            wsProd.Cell(currProdRow, 2).Value = string.Empty;
            wsProd.Cell(currProdRow, 3).Value = string.Empty;
            currProdRow++;

            ExcelRenderHelpers.CopyRowLayout(wsProd, prodHeaderPrototypeRow, currProdRow, prodLastColumn);
            wsProd.Cell(currProdRow, 1).Value = string.Empty;
            wsProd.Cell(currProdRow, 2).Value = "Nombre";
            wsProd.Cell(currProdRow, 3).Value = "Empresa o Institución";
            currProdRow++;

            if (tipo.Productos.Count > 0)
            {
                foreach (var prod in tipo.Productos)
                {
                    ExcelRenderHelpers.CopyRowLayout(wsProd, prodProductPrototypeRow, currProdRow, prodLastColumn);
                    wsProd.Cell(currProdRow, 1).Value = string.Empty;
                    wsProd.Cell(currProdRow, 2).Value = prod.Titulo;
                    wsProd.Cell(currProdRow, 3).Value = prod.InstitutionNombre;
                    currProdRow++;
                }
            }
            else
            {
                ExcelRenderHelpers.CopyRowLayout(wsProd, prodProductPrototypeRow, currProdRow, prodLastColumn);
                currProdRow++;
            }
        }

        ExcelRenderHelpers.ClearUnusedRows(wsProd, currProdRow, prodFirstBodyRow + bodyRowCountProd - 1, prodLastColumn);

        ClearTemplatePlaceholders(wsPat);
        ClearTemplatePlaceholders(wsReg);
        ClearTemplatePlaceholders(wsNormas);
        ClearTemplatePlaceholders(wsProd);

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }

    private static void ClearTemplatePlaceholders(IXLWorksheet ws)
    {
        var used = ws.RangeUsed();
        if (used == null) return;
        foreach (var cell in used.Cells())
        {
            if (cell.DataType == XLDataType.Text)
            {
                var s = cell.GetString();
                if (!string.IsNullOrEmpty(s) && s.Contains("{{") && s.Contains("}}"))
                    cell.Clear(XLClearOptions.Contents);
            }
        }
    }
}
