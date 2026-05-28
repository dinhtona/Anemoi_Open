using System.Collections.Generic;

namespace Anemoi.Contract.MasterData.Responses;

public sealed record SampleDataResponse(
    Dictionary<string, object> Data
);
