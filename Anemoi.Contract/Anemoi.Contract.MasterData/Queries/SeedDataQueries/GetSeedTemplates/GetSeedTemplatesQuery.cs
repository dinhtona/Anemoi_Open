using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Contract.MasterData.Responses;

namespace Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedTemplates;

public sealed record GetSeedTemplatesQuery(string SearchKey) : GetManyQuery, IQueryPaged<SeedTemplateResponse>;
