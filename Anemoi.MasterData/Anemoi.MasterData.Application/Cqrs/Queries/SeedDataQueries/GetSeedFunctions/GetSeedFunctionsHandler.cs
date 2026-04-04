using System.Linq.Expressions;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedFunctions;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Queries.SeedDataQueries.GetSeedFunctions;

public sealed class GetSeedFunctionsHandler(
    ISqlRepository<SeedFunction> sqlRepository,
    MasterDataMapper mapper,
    ILogger logger)
    : EfQueryPaginationHandler<SeedFunction, GetSeedFunctionsQuery, SeedFunctionResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<SeedFunction, SeedFunctionResponse> BuildQueryFlow(
        IQueryListFilter<SeedFunction, SeedFunctionResponse> fromFlow, GetSeedFunctionsQuery query)
    {
        Expression<Func<SeedFunction, bool>> searchFilter = query.SearchKey switch
        {
            { } val => x => x.Name.Contains(val),
            _ => _ => true
        };

        return fromFlow
            .WithFilter(searchFilter)
            .WithSpecialAction(x => mapper.ProjectToSeedFunctionResponse(x.OrderBy(s => s.Name)))
            .WithSortFieldWhenNotSet(x => x.Name)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
    }
}
