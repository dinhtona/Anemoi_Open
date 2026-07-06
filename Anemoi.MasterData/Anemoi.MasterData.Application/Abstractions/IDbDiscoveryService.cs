using Anemoi.Contract.MasterData.Responses;

namespace Anemoi.MasterData.Application.Abstractions;

public interface IDbDiscoveryService
{
    Task<DbSchemaResponse> GetSchemaAsync(string connectionString, string provider, CancellationToken cancellationToken = default);
    Task<TableSchema> GetTableSchemaAsync(string connectionString, string provider, string tableName, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(string connectionString, string provider, CancellationToken cancellationToken = default);
}
