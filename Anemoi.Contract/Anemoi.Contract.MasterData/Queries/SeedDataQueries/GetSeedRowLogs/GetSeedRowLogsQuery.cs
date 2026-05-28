using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Responses;
using System.Collections.Generic;

namespace Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedRowLogs;

public sealed record GetSeedRowLogsQuery(SeedServerId SeedServerId) : IQueryOne<SeedRowLogsResponse>;
