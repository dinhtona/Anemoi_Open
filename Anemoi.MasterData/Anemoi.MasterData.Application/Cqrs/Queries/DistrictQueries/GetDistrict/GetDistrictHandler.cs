using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.Contract.MasterData.Queries.DistrictQueries.GetDistrict;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Queries.DistrictQueries.GetDistrict;

public sealed class GetDistrictHandler(ISqlRepository<District> sqlRepository, MasterDataMapper mapper, ILogger logger)
    : EfQueryOneHandler<District, GetDistrictQuery, DistrictResponse>(sqlRepository, logger)
{
    protected override IQueryOneFlowBuilder<District, DistrictResponse> BuildQueryFlow(
        IQueryOneFilter<District, DistrictResponse> fromFlow, GetDistrictQuery query)
        => fromFlow
            .WithFilter(x => x.Id == query.Id)
            .WithSpecialAction(mapper.ProjectToDistrictResponse)
            .WithErrorIfNull(MasterDataErrorDetail.DistrictError.NotFound());
}