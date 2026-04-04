using Anemoi.Contract.MasterData.Responses;

namespace Anemoi.MasterData.Application.Abstractions;

public interface IDataGeneratorService
{
    Task<List<Dictionary<string, object>>> GenerateDataAsync(TableSchema tableSchema, int count, string configJson, CancellationToken cancellationToken = default);
}
