using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.MasterData.Commands.DistrictCommands.CreateDistrict;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.DistrictCommands.CreateDistrict;

public sealed class CreateDistrictHandler(
    ISqlRepository<District> sqlRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<District, CreateDistrictCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<District> BuildCommand(
        IStartOneCommandVoid<District> fromFlow, CreateDistrictCommand command,
        CancellationToken cancellationToken)
        => fromFlow
            .CreateOne(mapper.ToDistrict(command))
            .WithCondition(_ => None.Value)
            .WithErrorIfSaveChange(MasterDataErrorDetail.DistrictError.CreateFailed());
}