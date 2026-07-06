using System.Linq.Expressions;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.MasterData.Queries.ProvinceQueries.GetProvinces;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Queries.ProvinceQueries.GetProvinces;

public sealed class GetProvincesHandler(ISqlRepository<Province> sqlRepository, MasterDataMapper mapper, ILogger logger)
    : EfQueryPaginationHandler<Province, GetProvincesQuery, ProvinceResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<Province, ProvinceResponse> BuildQueryFlow(
        IQueryListFilter<Province, ProvinceResponse> fromFlow, GetProvincesQuery query)
    {
        Expression<Func<Province, bool>> searchKeyFilter = query.SearchKey switch
        {
            { } val => p => p.SearchHint.Contains(val.GenerateSearchHint()),
            _ => _ => true
        };
        return fromFlow
            .WithFilter(searchKeyFilter)
            .WithSpecialAction(x => mapper.ProjectToProvinceResponse(x
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.Name)))
            .WithSortFieldWhenNotSet(x => x.Priority)
            .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);
    }
}