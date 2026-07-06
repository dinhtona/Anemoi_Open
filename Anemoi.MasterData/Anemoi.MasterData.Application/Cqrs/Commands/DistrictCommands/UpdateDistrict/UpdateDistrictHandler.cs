using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.MasterData.Commands.DistrictCommands.UpdateDistrict;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.DistrictCommands.UpdateDistrict;

public class UpdateDistrictHandler(
    ISqlRepository<District> sqlRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<District, UpdateDistrictCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<District> BuildCommand(
        IStartOneCommandVoid<District> fromFlow, UpdateDistrictCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .UpdateOne(x => x.Id == command.Id)
            .WithSpecialAction(null)
            .WithCondition(_ => None.Value)
            .WithModify(district => mapper.UpdateDistrict(command, district))
            .WithErrorIfNull(MasterDataErrorDetail.DistrictError.NotFound())
            .WithErrorIfSaveChange(MasterDataErrorDetail.DistrictError.UpdateFailed());
}