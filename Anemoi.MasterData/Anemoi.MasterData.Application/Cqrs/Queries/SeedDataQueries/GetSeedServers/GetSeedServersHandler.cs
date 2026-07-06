using System.Linq.Expressions;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedServers;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Queries.SeedDataQueries.GetSeedServers;

public sealed class GetSeedServersHandler(
    ISqlRepository<SeedServer> sqlRepository,
    MasterDataMapper mapper,
    ILogger logger)
    : EfQueryPaginationHandler<SeedServer, GetSeedServersQuery, SeedServerResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<SeedServer, SeedServerResponse> BuildQueryFlow(
        IQueryListFilter<SeedServer, SeedServerResponse> fromFlow, GetSeedServersQuery query)
    {
        Expression<Func<SeedServer, bool>> searchFilter = query.SearchKey switch
        {
            { } val => x => x.Name.Contains(val),
            _ => _ => true
        };

        return fromFlow
            .WithFilter(searchFilter)
            .WithSpecialAction(x => mapper.ProjectToSeedServerResponse(x.OrderBy(s => s.Name)))
            .WithSortFieldWhenNotSet(x => x.Name)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
    }
}
