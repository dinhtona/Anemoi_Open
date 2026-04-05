using Anemoi.Contract.MasterData.Responses;

namespace Anemoi.MasterData.Application.Abstractions;

public interface IDbDiscoveryService
{
    Task<DbSchemaResponse> GetSchemaAsync(string connectionString, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(string connectionString, CancellationToken cancellationToken = default);
}
