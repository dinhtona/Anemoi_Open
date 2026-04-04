using System.Linq.Expressions;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.MasterData.Queries.SeedDataQueries.GetSeedTemplates;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Queries.SeedDataQueries.GetSeedTemplates;

public sealed class GetSeedTemplatesHandler(
    ISqlRepository<SeedTemplate> sqlRepository,
    MasterDataMapper mapper,
    ILogger logger)
    : EfQueryPaginationHandler<SeedTemplate, GetSeedTemplatesQuery, SeedTemplateResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<SeedTemplate, SeedTemplateResponse> BuildQueryFlow(
        IQueryListFilter<SeedTemplate, SeedTemplateResponse> fromFlow, GetSeedTemplatesQuery query)
    {
        Expression<Func<SeedTemplate, bool>> searchFilter = query.SearchKey switch
        {
            { } val => x => x.Name.Contains(val),
            _ => _ => true
        };

        return fromFlow
            .WithFilter(searchFilter)
            .WithSpecialAction(x => mapper.ProjectToSeedTemplateResponse(x.OrderBy(s => s.Name)))
            .WithSortFieldWhenNotSet(x => x.Name)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
    }
}
