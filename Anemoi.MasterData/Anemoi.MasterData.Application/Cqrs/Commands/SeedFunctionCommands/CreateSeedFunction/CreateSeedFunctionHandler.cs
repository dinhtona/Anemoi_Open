using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.CreateSeedFunction;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedFunctionCommands.CreateSeedFunction;

public sealed class CreateSeedFunctionHandler(
    ISqlRepository<SeedFunction> sqlRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<SeedFunction, CreateSeedFunctionCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<SeedFunction> BuildCommand(
        IStartOneCommandVoid<SeedFunction> fromFlow, CreateSeedFunctionCommand command,
        CancellationToken cancellationToken) => fromFlow
        .CreateOne(mapper.ToSeedFunction(command))
        .WithCondition(async _ =>
        {
            var exists = await SqlRepository.ExistByConditionAsync(x => x.Name == command.Name, cancellationToken);
            return exists ? MasterDataErrorDetail.SeedFunctionError.AlreadyExist() : None.Value;
        })
        .WithErrorIfSaveChange(MasterDataErrorDetail.SeedFunctionError.CreateFailed());
}
