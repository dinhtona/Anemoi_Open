namespace Anemoi.MasterData.Application.Abstractions;

public interface IDataIngestionService
{
    Task IngestDataAsync(string connectionString, string provider, string tableName, List<Dictionary<string, object>> data, CancellationToken cancellationToken = default);
    Task UpdateColumnDataAsync(string connectionString, string provider, string tableName, string keyColumn, string updateColumn, Dictionary<object, object> updateData, CancellationToken cancellationToken = default);
}
