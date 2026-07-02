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

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.UpdateEmployeeAsset;

public sealed class UpdateEmployeeAssetHandler(
    ISqlRepository<EmployeeAsset> assetRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEmployeeAssetCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        UpdateEmployeeAssetCommand request, CancellationToken ct)
    {
        var asset = await assetRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, ct);
        if (asset is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeAssetNotFound);

        if (string.IsNullOrWhiteSpace(request.Name))
            return HrErrorResponses.Create(HrBusinessErrorCodes.DisplayNameRequired);

        asset.UpdateInfo(
            request.Name,
            request.Brand,
            request.Model,
            request.SerialNumber,
            request.Notes);

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = asset.EmployeeId,
            EntityType = "EmployeeAsset",
            EntityId = request.Id.Value.ToString(),
            EventType = "Updated",
            Title = "Employee asset updated",
            Description = request.Name,
            OccurredAt = DateTime.UtcNow,
            ActorUserId = Guid.TryParse(request.UpdatedBy, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new None();
    }
}
