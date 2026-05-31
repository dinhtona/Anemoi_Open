using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.MasterData.Commands.SeedServerCommands.UpdateSeedServer;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedServerCommands.UpdateSeedServer;

public sealed class UpdateSeedServerHandler(
    ISqlRepository<SeedServer> sqlRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<SeedServer, UpdateSeedServerCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<SeedServer> BuildCommand(
        IStartOneCommandVoid<SeedServer> fromFlow, UpdateSeedServerCommand command,
        CancellationToken cancellationToken) => fromFlow
        .UpdateOne(x => x.Id == command.Id)
        .WithSpecialAction(null)
        .WithCondition(async server =>
        {
            var exists = await SqlRepository.ExistByConditionAsync(x => x.Name == command.Name && x.Id != server.Id, cancellationToken);
            return exists ? MasterDataErrorDetail.SeedServerError.AlreadyExist() : None.Value;
        })
        .WithModify(x => mapper.UpdateSeedServer(command, x))
        .WithErrorIfNull(MasterDataErrorDetail.SeedServerError.NotFound())
        .WithErrorIfSaveChange(MasterDataErrorDetail.SeedServerError.UpdateFailed());
}
