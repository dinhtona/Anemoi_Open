#nullable enable

using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Abstractions;

public interface IReportExporter
{
    ExportResult ExportCsv<T>(IReadOnlyCollection<T> records, string fileName) where T : class;
}
