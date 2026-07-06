using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.QueryHelpers;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Queries.ProvinceQueries.GetCrossCuttingProvinces;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Queries.ProvinceQueries.GetCrossCuttingProvinces;

public sealed class GetCrossCuttingProvincesHandler(
    ISqlRepository<Province> sqlRepository,
    ILogger logger)
    : EfQueryCrossCuttingHandler<Province, GetCrossCuttingProvincesQuery>(sqlRepository, logger,
        x => d => x.SelectorIds.Select(a => new ProvinceId(a)).Contains(d.Id),
        x => new CrossCuttingDataResponse { Id = x.Id.ToString(), Value = x.Name });
