using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.MasterData.Application.Abstractions;

public interface IDataGeneratorService
{
    Task<List<Dictionary<string, object>>> GenerateDataAsync(
        TableSchema tableSchema, 
        TableConfig tableConfig,
        List<RelationshipConfig> activeRelationships,
        Dictionary<string, List<Dictionary<string, object>>> seedingContext, 
        CancellationToken cancellationToken = default);
}
