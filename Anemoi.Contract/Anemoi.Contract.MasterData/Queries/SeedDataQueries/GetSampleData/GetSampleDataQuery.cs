using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.MasterData.ModelIds;
using OneOf;
using System.Collections.Generic;

namespace Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSampleData;

public sealed record GetSampleDataQuery(SeedFunctionId SeedFunctionId, string TableName) : IQueryOne<Dictionary<string, object>>;
