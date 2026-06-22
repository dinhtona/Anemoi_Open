using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Employees.Events;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;

namespace Anemoi.Hr.Application.EventHandlers;

public sealed class EmployeeStatusChangedHistoryHandler(
    ISqlRepository<EmployeeHistory> historyRepo,
    IUnitOfWork unitOfWork)
    : INotificationHandler<EmployeeStatusChangedDomainEvent>
{
    public async Task Handle(EmployeeStatusChangedDomainEvent notification, CancellationToken ct)
    {
        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = notification.EmployeeId,
            EntityType = "Employee",
            EntityId = notification.EmployeeId.ToString()!,
            EventType = "EmployeeStatusChanged",
            Title = "Status Changed",
            Description = $"Status: {notification.FromStatus} → {notification.ToStatus}",
            MetadataJson = "{}",
            OccurredAt = DateTime.UtcNow,
            CorrelationId = notification.CorrelationId ?? string.Empty
        };
        await historyRepo.CreateOneAsync(history, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
