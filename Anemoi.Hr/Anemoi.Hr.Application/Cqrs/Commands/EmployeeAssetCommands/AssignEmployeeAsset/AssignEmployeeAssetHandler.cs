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

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.AssignEmployeeAsset;

public sealed class AssignEmployeeAssetHandler(
    ISqlRepository<EmployeeAsset> assetRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignEmployeeAssetCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        AssignEmployeeAssetCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.AssetType))
            return HrErrorResponses.Create(HrBusinessErrorCodes.AssetTypeRequired);

        if (string.IsNullOrWhiteSpace(request.AssetTag))
            return HrErrorResponses.Create(HrBusinessErrorCodes.AssetTagRequired);

        if (string.IsNullOrWhiteSpace(request.Name))
            return HrErrorResponses.Create(HrBusinessErrorCodes.DisplayNameRequired);

        AssetType assetType;
        try
        {
            assetType = AssetType.FromValue(request.AssetType);
        }
        catch (ArgumentException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.AssetTypeRequired);
        }

        var assetId = new EmployeeAssetId(IdGenerator.NextGuid());
        var now = DateTime.UtcNow;

        var asset = EmployeeAsset.Create(
            assetId,
            assetType,
            request.AssetTag,
            request.Name,
            request.Brand,
            request.Model,
            request.SerialNumber,
            request.Notes);

        asset.Assign(request.EmployeeId);

        await assetRepository.CreateOneAsync(asset, ct);

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            EntityType = "EmployeeAsset",
            EntityId = assetId.Value.ToString(),
            EventType = "Assigned",
            Title = "Employee asset assigned",
            Description = request.Name,
            MetadataJson = "{}",
            CorrelationId = "",
            OccurredAt = now,
            ActorUserId = Guid.TryParse(request.CreatedBy, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new None();
    }
}
