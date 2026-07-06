using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation.Events;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;

namespace Anemoi.Hr.Application.EventHandlers;

public sealed class ProbationStatusChangedHistoryHandler(
    ISqlRepository<EmployeeHistory> historyRepo,
    IUnitOfWork unitOfWork)
    : INotificationHandler<ProbationStatusChangedDomainEvent>
{
    public async Task Handle(ProbationStatusChangedDomainEvent notification, CancellationToken ct)
    {
        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = notification.EmployeeId,
            EntityType = "Probation",
            EntityId = notification.RecordId.ToString()!,
            EventType = "ProbationStatusChanged",
            Title = "Probation Status Changed",
            Description = $"Probation {notification.ToStatus}",
            MetadataJson = "{}",
            OccurredAt = DateTime.UtcNow,
            CorrelationId = notification.CorrelationId ?? string.Empty
        };
        await historyRepo.CreateOneAsync(history, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
