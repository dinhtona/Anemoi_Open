using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Transfers.Events;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;

namespace Anemoi.Hr.Application.EventHandlers;

public sealed class TransferApprovedHistoryHandler(
    ISqlRepository<EmployeeHistory> historyRepo,
    IUnitOfWork unitOfWork)
    : INotificationHandler<TransferApprovedDomainEvent>
{
    public async Task Handle(TransferApprovedDomainEvent notification, CancellationToken ct)
    {
        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = notification.EmployeeId,
            EntityType = "Transfer",
            EntityId = notification.TransferId.ToString()!,
            EventType = "TransferApproved",
            Title = "Transfer Approved",
            Description = "Transfer Approved",
            MetadataJson = JsonSerializer.Serialize(new
            {
                transferId = notification.TransferId
            }),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = notification.CorrelationId ?? string.Empty
        };
        await historyRepo.CreateOneAsync(history, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
