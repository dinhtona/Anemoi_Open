using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.QueryHelpers;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Queries.DistrictQueries.GetCrossCuttingDistricts;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Queries.DistrictQueries.GetCrossCuttingDistricts;

public sealed class GetCrossCuttingDistrictsHandler(
    ISqlRepository<District> sqlRepository,
    ILogger logger)
    : EfQueryCrossCuttingHandler<District, GetCrossCuttingDistrictsQuery>(sqlRepository, logger,
        x => d => x.SelectorIds.Select(a => new DistrictId(a)).Contains(d.Id),
        x => new CrossCuttingDataResponse { Id = x.Id.ToString(), Value = x.Name });
