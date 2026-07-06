namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeNoteResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string Content { get; set; }
    public string NoteCategory { get; set; }
    public string CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsArchived { get; set; }
}
