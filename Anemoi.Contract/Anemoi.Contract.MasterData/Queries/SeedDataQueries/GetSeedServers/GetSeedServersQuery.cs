using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Contract.MasterData.Responses;

namespace Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedServers;

public sealed record GetSeedServersQuery(string SearchKey) : GetManyQuery, IQueryPaged<SeedServerResponse>;
