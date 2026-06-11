#nullable enable

using System.Globalization;
using System.Reflection;
using System.Text;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Infrastructure.Reporting;

public sealed class CsvReportExporter : IReportExporter
{
    public ExportResult ExportCsv<T>(IReadOnlyCollection<T> records, string fileName) where T : class
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.CanRead)
            .ToArray();
        var csv = new StringBuilder();

        csv.AppendLine(string.Join(',', properties.Select(x => Escape(x.Name))));
        foreach (var record in records)
        {
            csv.AppendLine(string.Join(',', properties.Select(x => Escape(FormatValue(x.GetValue(record))))));
        }

        return new ExportResult(
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv.ToString()),
            "text/csv; charset=utf-8",
            fileName);
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string Escape(string value)
    {
        if (value.IndexOfAny([',', '"', '\r', '\n']) < 0)
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
