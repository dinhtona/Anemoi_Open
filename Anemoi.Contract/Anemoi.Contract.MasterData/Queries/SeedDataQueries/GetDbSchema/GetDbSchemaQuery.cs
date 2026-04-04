using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Responses;

namespace Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetDbSchema;

public sealed record GetDbSchemaQuery(SeedServerId SeedServerId) : IQueryOne<DbSchemaResponse>;

