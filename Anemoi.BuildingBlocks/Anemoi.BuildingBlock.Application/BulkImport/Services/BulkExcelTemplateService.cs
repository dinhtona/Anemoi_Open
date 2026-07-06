#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Anemoi.BuildingBlock.Application.BulkImport.Services;

public sealed class BulkExcelTemplateService
{
    public byte[] GenerateTemplate(IReadOnlyCollection<ImportColumnDefinition> columns)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Template");

        using var hr = ws.Cells[1, 1, 1, columns.Count];
        hr.Style.Font.Bold = true;
        hr.Style.Fill.PatternType = ExcelFillStyle.Solid;
        hr.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
        hr.Style.Font.Color.SetColor(Color.White);

        var i = 0;
        foreach (var col in columns)
        {
            var cell = ws.Cells[1, ++i];
            cell.Value = col.Header;
            if (!string.IsNullOrEmpty(col.ValidationNote))
                ws.Comments.Add(cell, col.ValidationNote, "System");
            if (!string.IsNullOrEmpty(col.ExampleValue))
            {
                ws.Cells[2, i].Value = col.ExampleValue;
                if (col.IsRequired)
                    ws.Cells[2, i].Style.Font.Color.SetColor(Color.Red);
            }
            ws.Column(i).AutoFit();
        }
        ws.View.FreezePanes(2, 1);
        return pkg.GetAsByteArray();
    }

    public byte[] GenerateErrorFile(
        IReadOnlyCollection<ImportPreviewRow> previewRows,
        IReadOnlyCollection<BulkImportRowResult> rowResults,
        IReadOnlyCollection<ImportColumnDefinition> columns)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Errors");
        var colList = columns.ToList();

        for (var c = 0; c < colList.Count; c++)
            ws.Cells[1, c + 1].Value = colList[c].Header;

        var errCol = colList.Count + 1;
        ws.Cells[1, errCol].Value = "Error Message";
        ws.Cells[1, errCol].Style.Font.Bold = true;
        ws.Cells[1, errCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells[1, errCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 0, 0));
        ws.Cells[1, errCol].Style.Font.Color.SetColor(Color.White);

        var errMap = rowResults
            .Where(r => r.Errors?.Count > 0)
            .ToDictionary(r => r.RowIndex, r => r.Errors);

        var rowList = previewRows.ToList();
        for (var i = 0; i < rowList.Count; i++)
        {
            var row = rowList[i];
            for (var c = 0; c < colList.Count; c++)
                ws.Cells[i + 2, c + 1].Value =
                    row.Data.GetValueOrDefault(colList[c].BusinessName, "");

            if (errMap.TryGetValue(i, out var errors) && errors != null)
            {
                ws.Cells[i + 2, errCol].Value = string.Join("; ",
                    errors.Select(e => $"[{e.Severity}] {e.Column}: {e.ErrorMessage}"));
                ws.Cells[i + 2, errCol].Style.Font.Color.SetColor(Color.Red);
            }
        }

        ws.Column(errCol).AutoFit();
        for (var c = 1; c <= colList.Count; c++) ws.Column(c).AutoFit();
        return pkg.GetAsByteArray();
    }
}
