using System.Text.Json;
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

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.MarkAssetDamaged;

public sealed class MarkAssetDamagedHandler(
    ISqlRepository<EmployeeAsset> assetRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MarkAssetDamagedCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        MarkAssetDamagedCommand request, CancellationToken ct)
    {
        var asset = await assetRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, ct);
        if (asset is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeAssetNotFound);

        asset.MarkDamaged();

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = asset.EmployeeId,
            EntityType = "EmployeeAsset",
            EntityId = request.Id.Value.ToString(),
            EventType = "MarkedDamaged",
            Title = "Employee asset marked as damaged",
            Description = asset.Name,
            MetadataJson = JsonSerializer.Serialize(new { name = asset.Name }),
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
