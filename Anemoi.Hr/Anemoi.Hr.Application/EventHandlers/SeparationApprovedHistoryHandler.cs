using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Separations;
using Anemoi.Hr.Domain.Separations.Events;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;

namespace Anemoi.Hr.Application.EventHandlers;

public sealed class SeparationApprovedHistoryHandler(
    ISqlRepository<EmployeeHistory> historyRepo,
    ISqlRepository<EmployeeSeparation> separationRepo,
    IUnitOfWork unitOfWork)
    : INotificationHandler<SeparationApprovedDomainEvent>
{
    public async Task Handle(SeparationApprovedDomainEvent notification, CancellationToken ct)
    {
        var separation = await separationRepo.GetFirstByConditionAsync(
            x => x.Id == notification.SeparationId, null, ct);
        var typeCode = separation?.SeparationTypeCode ?? "Unknown";

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = notification.EmployeeId,
            EntityType = "Separation",
            EntityId = notification.SeparationId.ToString()!,
            EventType = "SeparationApproved",
            Title = "Separation Approved",
            Description = $"Separation: {typeCode}",
            MetadataJson = JsonSerializer.Serialize(new
            {
                separationId = notification.SeparationId,
                separationTypeCode = typeCode
            }),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = notification.CorrelationId ?? string.Empty
        };
        await historyRepo.CreateOneAsync(history, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
