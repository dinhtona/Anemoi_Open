using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;

namespace Anemoi.BuildingBlock.Application.BulkImport.Services;

public sealed class BulkExcelParserService
{
    public IReadOnlyCollection<Dictionary<string, string>> Parse(byte[] fileBytes, string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".xlsx" => ParseXlsx(fileBytes),
            ".csv" => ParseCsv(fileBytes),
            _ => throw new System.ArgumentException($"Unsupported format: {ext}")
        };
    }

    private static List<Dictionary<string, string>> ParseXlsx(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var pkg = new ExcelPackage(stream);
        var ws = pkg.Workbook.Worksheets[0];
        var rowCount = ws.Dimension?.Rows ?? 0;
        var colCount = ws.Dimension?.Columns ?? 0;
        if (rowCount < 2) return [];

        var headers = new List<string>();
        for (var c = 1; c <= colCount; c++)
        {
            var h = ws.Cells[1, c].Text?.Trim();
            if (!string.IsNullOrEmpty(h)) headers.Add(h);
        }

        var result = new List<Dictionary<string, string>>(rowCount - 1);
        for (var r = 2; r <= rowCount; r++)
        {
            var row = new Dictionary<string, string>(headers.Count);
            var empty = true;
            for (var c = 0; c < headers.Count; c++)
            {
                var val = ws.Cells[r, c + 1].Text?.Trim() ?? "";
                row[headers[c]] = val;
                if (val.Length > 0) empty = false;
            }
            if (!empty) result.Add(row);
        }
        return result;
    }

    private static List<Dictionary<string, string>> ParseCsv(byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var lines = text.Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => l.Length > 0)
            .ToList();
        if (lines.Count < 2) return [];

        var headers = SplitCsvLine(lines[0])
            .Select(h => h.Trim('"', ' '))
            .ToList();

        var result = new List<Dictionary<string, string>>(lines.Count - 1);
        for (var i = 1; i < lines.Count; i++)
        {
            var values = SplitCsvLine(lines[i]);
            var row = new Dictionary<string, string>(headers.Count);
            var empty = true;
            for (var c = 0; c < headers.Count && c < values.Count; c++)
            {
                row[headers[c]] = values[c].Trim('"', ' ');
                if (values[c].Length > 0) empty = false;
            }
            if (!empty) result.Add(row);
        }
        return result;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var cur = new StringBuilder();
        var inQ = false;
        foreach (var ch in line)
        {
            if (ch == '"') inQ = !inQ;
            else if (ch == ',' && !inQ) { result.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(ch);
        }
        result.Add(cur.ToString());
        return result;
    }
}
