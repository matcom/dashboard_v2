using ClosedXML.Excel;

namespace TemplateGen.Templates.Anexo1;

/// <summary>
/// Utilidades compartidas para las hojas del Anexo Resumen (Anexo 1).
/// Todas las hojas son tablas de escalares con filas fijas; solo la hoja de Premios
/// usa un rango dinámico (Named Range "Premios") para la lista de tipos.
/// </summary>
internal static class Anexo1SheetHelper
{
    internal static readonly XLColor BlueHeader  = XLColor.FromHtml("#BDD7EE");
    internal static readonly XLColor BlueLabel   = XLColor.FromHtml("#D9E1F2");
    internal static readonly XLColor GreySection = XLColor.FromHtml("#F2F2F2");
    internal static readonly XLColor Yellow      = XLColor.FromHtml("#FFEB9C");

    // ── Column widths ────────────────────────────────────────────────────────

    internal static void SetWidths(IXLWorksheet ws, params double[] widths)
    {
        for (int i = 0; i < widths.Length; i++)
            ws.Column(i + 1).Width = widths[i];
    }

    // ── Title row ────────────────────────────────────────────────────────────

    internal static void WriteTitle(IXLWorksheet ws, int row, string text, int colCount)
    {
        var cell = ws.Cell(row, 1);
        cell.Value = text;
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 11;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        if (colCount > 1)
            ws.Range(row, 1, row, colCount).Merge();
    }

    // ── Header row ───────────────────────────────────────────────────────────

    internal static void WriteHeaderRow(IXLWorksheet ws, int row, params string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(row, i + 1);
            cell.Value = headers[i];
            var s = cell.Style;
            s.Font.Bold = true;
            s.Fill.BackgroundColor = BlueHeader;
            s.Border.OutsideBorder = XLBorderStyleValues.Thin;
            s.Alignment.WrapText = true;
            s.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            s.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        }
        ws.Row(row).Height = 36;
    }

    // ── Merged header (for multi-level headers) ──────────────────────────────

    internal static void WriteMergedHeader(IXLWorksheet ws, int row, int fromCol, int toCol, string text)
    {
        var cell = ws.Cell(row, fromCol);
        cell.Value = text;
        var s = cell.Style;
        s.Font.Bold = true;
        s.Fill.BackgroundColor = BlueHeader;
        s.Border.OutsideBorder = XLBorderStyleValues.Thin;
        s.Alignment.WrapText   = true;
        s.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        s.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        if (toCol > fromCol)
            ws.Range(row, fromCol, row, toCol).Merge();
    }

    // ── Label cell (left-aligned, blue background) ───────────────────────────

    internal static void WriteLabel(IXLWorksheet ws, int row, int col, string text, bool bold = false)
    {
        var cell = ws.Cell(row, col);
        cell.Value = text;
        var s = cell.Style;
        s.Font.Bold = bold;
        s.Fill.BackgroundColor = BlueLabel;
        s.Border.OutsideBorder = XLBorderStyleValues.Thin;
        s.Alignment.WrapText   = true;
        s.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        s.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
    }

    // ── Section divider (grey background, bold) ───────────────────────────────

    internal static void WriteSection(IXLWorksheet ws, int row, int col, string text)
    {
        var cell = ws.Cell(row, col);
        cell.Value = text;
        var s = cell.Style;
        s.Font.Bold = true;
        s.Fill.BackgroundColor = GreySection;
        s.Border.OutsideBorder = XLBorderStyleValues.Thin;
        s.Alignment.WrapText   = true;
        s.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        s.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
    }

    // ── Empty value cell (bordered, centred) ─────────────────────────────────

    internal static void WriteBlank(IXLWorksheet ws, int row, int col)
    {
        var cell = ws.Cell(row, col);
        cell.Value = string.Empty;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    // ── Template scalar cell (yellow, centred) ───────────────────────────────

    internal static void WriteScalar(IXLWorksheet ws, int row, int col, string varName)
    {
        var cell = ws.Cell(row, col);
        cell.Value = $"{{{{{varName}}}}}";
        var s = cell.Style;
        s.Fill.BackgroundColor = Yellow;
        s.Border.OutsideBorder = XLBorderStyleValues.Thin;
        s.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        s.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
    }

    // ── Full data row: label (col1) + optional blank/scalar cells ────────────

    internal static void WriteDataRow(
        IXLWorksheet ws,
        int row,
        string label,
        int totalCols,
        (int Col, string? VarName)[]? scalars = null)
    {
        WriteLabel(ws, row, 1, label);
        for (int c = 2; c <= totalCols; c++)
            WriteBlank(ws, row, c);

        if (scalars is not null)
            foreach (var (col, var) in scalars)
                if (var is not null)
                    WriteScalar(ws, row, col, var);
    }

    // ── List prototype rows + Named Range (for ClosedXML.Report dynamic list) ─

    internal static void WriteListRange(
        IXLWorksheet ws,
        string rangeName,
        int dataRow,
        int lastCol,
        (int Col, string Expression)[] cells)
    {
        // Prototype row
        for (int c = 1; c <= lastCol; c++)
        {
            var cell = ws.Cell(dataRow, c);
            cell.Value = string.Empty;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        }
        foreach (var (col, expr) in cells)
        {
            var cell = ws.Cell(dataRow, col);
            cell.Value = expr;
            cell.Style.Fill.BackgroundColor = Yellow;
            cell.Style.Alignment.Horizontal = col == 1
                ? XLAlignmentHorizontalValues.Left
                : XLAlignmentHorizontalValues.Center;
        }

        // Service (terminator) row
        for (int c = 1; c <= lastCol; c++)
        {
            var svc = ws.Cell(dataRow + 1, c);
            svc.Value = string.Empty;
            svc.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Named Range
        ws.Workbook.DefinedNames.Add(rangeName, ws.Range(dataRow, 1, dataRow + 1, lastCol));
    }
}
