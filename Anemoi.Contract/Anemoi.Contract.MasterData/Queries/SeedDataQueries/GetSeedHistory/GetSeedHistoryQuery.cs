using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Responses;
using System.Collections.Generic;

namespace Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedHistory;

public sealed record GetSeedHistoryQuery(SeedFunctionId SeedFunctionId) : IQuery<List<SeedHistoryResponse>>;
