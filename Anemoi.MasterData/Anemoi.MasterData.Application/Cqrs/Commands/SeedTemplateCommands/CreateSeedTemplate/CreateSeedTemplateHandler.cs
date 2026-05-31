using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Application.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.CreateSeedTemplate;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedTemplateCommands.CreateSeedTemplate;

public sealed class CreateSeedTemplateHandler(
    ISqlRepository<SeedTemplate> sqlRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<SeedTemplate, CreateSeedTemplateCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<SeedTemplate> BuildCommand(
        IStartOneCommandVoid<SeedTemplate> fromFlow, CreateSeedTemplateCommand command,
        CancellationToken cancellationToken) => fromFlow
        .CreateOne(mapper.ToSeedTemplate(command))
        .WithCondition(async _ =>
        {
            var exists = await SqlRepository.ExistByConditionAsync(x => x.Name == command.Name, cancellationToken);
            return exists ? MasterDataErrorDetail.SeedTemplateError.AlreadyExist() : None.Value;
        })
        .WithErrorIfSaveChange(MasterDataErrorDetail.SeedTemplateError.CreateFailed());
}
