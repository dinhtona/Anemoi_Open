using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Employees;

namespace Anemoi.Hr.Application.Mappings;

public sealed class EmployeeHistoryMapper
{
    public EmployeeHistoryDto ToDto(EmployeeHistory history)
    {
        if (history is null) return null;
        return new EmployeeHistoryDto
        {
            Id = history.Id.Value.ToString(),
            EmployeeId = history.EmployeeId.Value.ToString(),
            EntityType = history.EntityType,
            EntityId = history.EntityId,
            EventType = history.EventType,
            Title = history.Title,
            Description = history.Description,
            MetadataJson = history.MetadataJson,
            OccurredAt = history.OccurredAt,
            ActorUserId = history.ActorUserId?.ToString(),
            ActorEmployeeId = history.ActorEmployeeId?.Value.ToString(),
            CorrelationId = history.CorrelationId
        };
    }
}
