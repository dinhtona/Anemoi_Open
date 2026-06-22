using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Employees.Events;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;

namespace Anemoi.Hr.Application.EventHandlers;

public sealed class EmployeeCreatedHistoryHandler(
    ISqlRepository<EmployeeHistory> historyRepo,
    IUnitOfWork unitOfWork)
    : INotificationHandler<EmployeeCreatedDomainEvent>
{
    public async Task Handle(EmployeeCreatedDomainEvent notification, CancellationToken ct)
    {
        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = notification.EmployeeId,
            EntityType = "Employee",
            EntityId = notification.EmployeeId.ToString()!,
            EventType = "EmployeeCreated",
            Title = "Employee Created",
            Description = $"Employee {notification.FullName} ({notification.EmployeeCode}) created",
            MetadataJson = "{}",
            OccurredAt = DateTime.UtcNow,
            CorrelationId = notification.CorrelationId ?? string.Empty
        };
        await historyRepo.CreateOneAsync(history, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
