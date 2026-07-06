using System.Collections.Generic;

namespace Anemoi.Contract.MasterData.Responses;

public sealed record TriggerSeedingResponse(
    Dictionary<string, List<Dictionary<string, object>>> TableData
);
