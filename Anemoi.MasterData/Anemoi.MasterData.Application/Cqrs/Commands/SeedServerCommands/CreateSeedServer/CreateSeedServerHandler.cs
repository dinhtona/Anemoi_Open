using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.MasterData.Commands.SeedServerCommands.CreateSeedServer;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedServerCommands.CreateSeedServer;

public sealed class CreateSeedServerHandler(
    ISqlRepository<SeedServer> sqlRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<SeedServer, CreateSeedServerCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<SeedServer> BuildCommand(
        IStartOneCommandVoid<SeedServer> fromFlow, CreateSeedServerCommand command,
        CancellationToken cancellationToken) => fromFlow
        .CreateOne(mapper.ToSeedServer(command))
        .WithCondition(async _ =>
        {
            var exists = await SqlRepository.ExistByConditionAsync(x => x.Name == command.Name, cancellationToken);
            return exists ? MasterDataErrorDetail.SeedServerError.AlreadyExist() : None.Value;
        })
        .WithErrorIfSaveChange(MasterDataErrorDetail.SeedServerError.CreateFailed());
}
