namespace Anemoi.Hr.Application.Cqrs.Common.Dtos;

public sealed record EmployeeHistoryDto
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string EventType { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? ActorUserId { get; set; }
    public string? ActorEmployeeId { get; set; }
    public string? CorrelationId { get; set; }
}
