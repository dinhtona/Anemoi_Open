using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.UpdateSeedFunction;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedFunctionCommands.UpdateSeedFunction;

public sealed class UpdateSeedFunctionHandler(
    ISqlRepository<SeedFunction> sqlRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<SeedFunction, UpdateSeedFunctionCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<SeedFunction> BuildCommand(
        IStartOneCommandVoid<SeedFunction> fromFlow, UpdateSeedFunctionCommand command,
        CancellationToken cancellationToken) => fromFlow
        .UpdateOne(x => x.Id == command.Id)
        .WithSpecialAction(null)
        .WithCondition(async function =>
        {
            var exists = await SqlRepository.ExistByConditionAsync(x => x.Name == command.Name && x.Id != function.Id, cancellationToken);
            return exists ? MasterDataErrorDetail.SeedFunctionError.AlreadyExist() : None.Value;
        })
        .WithModify(x => mapper.UpdateSeedFunction(command, x))
        .WithErrorIfNull(MasterDataErrorDetail.SeedFunctionError.NotFound())
        .WithErrorIfSaveChange(MasterDataErrorDetail.SeedFunctionError.UpdateFailed());
}
