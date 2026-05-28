using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Responses;

namespace Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSampleData;

public sealed record GetSampleDataQuery(SeedFunctionId SeedFunctionId, string TableName) : IQueryOne<SampleDataResponse>;

