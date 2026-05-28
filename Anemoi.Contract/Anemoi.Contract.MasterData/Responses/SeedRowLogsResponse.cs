using System.Collections.Generic;

namespace Anemoi.Contract.MasterData.Responses;

public sealed record SeedRowLogsResponse(
    List<SeedRowLogResponse> Logs
);
