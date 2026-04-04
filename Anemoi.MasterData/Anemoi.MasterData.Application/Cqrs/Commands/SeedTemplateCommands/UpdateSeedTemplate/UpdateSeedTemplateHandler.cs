using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands.CommandFlow.CommandOneFlow;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Commands.EntityFramework.EfCommandOne;
using Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.UpdateSeedTemplate;
using Anemoi.Contract.MasterData.Errors;
using Anemoi.MasterData.Application.Mappings;
using Anemoi.MasterData.Domain.Models;
using Serilog;

namespace Anemoi.MasterData.Application.Cqrs.Commands.SeedTemplateCommands.UpdateSeedTemplate;

public sealed class UpdateSeedTemplateHandler(
    ISqlRepository<SeedTemplate> sqlRepository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper,
    ILogger logger)
    : EfCommandOneVoidHandler<SeedTemplate, UpdateSeedTemplateCommand>(sqlRepository, unitOfWork, logger)
{
    protected override ICommandOneFlowBuilderVoid<SeedTemplate> BuildCommand(
        IStartOneCommandVoid<SeedTemplate> fromFlow, UpdateSeedTemplateCommand command,
        CancellationToken cancellationToken) => fromFlow
        .UpdateOne(x => x.Id == command.Id)
        .WithSpecialAction(null)
        .WithCondition(async template =>
        {
            var exists = await SqlRepository.ExistByConditionAsync(x => x.Name == command.Name && x.Id != template.Id, cancellationToken);
            return exists ? MasterDataErrorDetail.SeedTemplateError.AlreadyExist() : None.Value;
        })
        .WithModify(x => mapper.UpdateSeedTemplate(command, x))
        .WithErrorIfNull(MasterDataErrorDetail.SeedTemplateError.NotFound())
        .WithErrorIfSaveChange(MasterDataErrorDetail.SeedTemplateError.UpdateFailed());
}
