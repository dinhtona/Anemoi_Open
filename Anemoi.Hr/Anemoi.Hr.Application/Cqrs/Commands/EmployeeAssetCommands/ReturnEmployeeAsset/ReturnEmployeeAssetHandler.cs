using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.EmployeeAssets;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.ReturnEmployeeAsset;

public sealed class ReturnEmployeeAssetHandler(
    ISqlRepository<EmployeeAsset> assetRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReturnEmployeeAssetCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        ReturnEmployeeAssetCommand request, CancellationToken ct)
    {
        var asset = await assetRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, ct);
        if (asset is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeAssetNotFound);

        var employeeId = asset.EmployeeId;
        asset.Return();

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = employeeId,
            EntityType = "EmployeeAsset",
            EntityId = request.Id.Value.ToString(),
            EventType = "Returned",
            Title = "Employee asset returned",
            Description = asset.Name,
            OccurredAt = DateTime.UtcNow,
            ActorUserId = Guid.TryParse(request.ReturnedBy, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new None();
    }
}
