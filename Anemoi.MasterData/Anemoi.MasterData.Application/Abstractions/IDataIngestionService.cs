namespace Anemoi.MasterData.Application.Abstractions;

public interface IDataIngestionService
{
    Task IngestDataAsync(string connectionString, string tableName, List<Dictionary<string, object>> data, CancellationToken cancellationToken = default);
}
