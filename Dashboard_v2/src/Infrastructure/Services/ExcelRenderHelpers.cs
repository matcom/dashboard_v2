using ClosedXML.Excel;

namespace Dashboard_v2.Infrastructure.Services;

internal static class ExcelRenderHelpers
{
    internal static void CopyRowLayout(IXLWorksheet ws, int prototypeRow, int targetRow, int lastColumn)
    {
        ws.Row(targetRow).Height = ws.Row(prototypeRow).Height;
        for (int col = 1; col <= lastColumn; col++)
        {
            var targetCell = ws.Cell(targetRow, col);
            var prototypeCell = ws.Cell(prototypeRow, col);
            targetCell.Clear(XLClearOptions.Contents);
            targetCell.Style = prototypeCell.Style;
        }
    }

    internal static void ClearUnusedRows(IXLWorksheet ws, int startRow, int endRow, int lastColumn)
    {
        if (startRow > endRow) return;
        for (int row = startRow; row <= endRow; row++)
            for (int col = 1; col <= lastColumn; col++)
                ws.Cell(row, col).Clear(XLClearOptions.Contents);
    }
}
