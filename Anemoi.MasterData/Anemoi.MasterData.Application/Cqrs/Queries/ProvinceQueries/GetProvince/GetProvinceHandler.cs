using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.Contract.MasterData.Queries.ProvinceQueries.GetProvince;
using Anemoi.Contract.MasterData.Responses;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Queries.ProvinceQueries.GetProvince;

public sealed class GetProvinceHandler(ISqlRepository<Province> sqlRepository, MasterDataMapper mapper, ILogger logger)
    : EfQueryOneHandler<Province, GetProvinceQuery, ProvinceResponse>(sqlRepository, logger)
{
    protected override IQueryOneFlowBuilder<Province, ProvinceResponse> BuildQueryFlow(
        IQueryOneFilter<Province, ProvinceResponse> fromFlow, GetProvinceQuery query)
        => fromFlow
            .WithFilter(x => x.Id == query.Id)
            .WithSpecialAction(mapper.ProjectToProvinceResponse)
            .WithErrorIfNull(MasterDataErrorDetail.ProvinceError.NotFound());
}