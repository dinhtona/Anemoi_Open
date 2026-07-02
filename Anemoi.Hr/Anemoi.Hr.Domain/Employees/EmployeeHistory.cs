using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Employees;

public sealed class EmployeeHistory : Entity<EmployeeHistoryId>
{
    public EmployeeId EmployeeId { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string EventType { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public DateTime OccurredAt { get; set; }
    public Guid? ActorUserId { get; set; }
    public EmployeeId? ActorEmployeeId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;

    public Employee Employee { get; set; }
}
