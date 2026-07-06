using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.EmployeeNotes;

public sealed class EmployeeNote : Entity<EmployeeNoteId>
{
    public EmployeeId EmployeeId { get; private set; }
    public string Content { get; private set; }
    public NoteCategory NoteCategory { get; private set; }
    public string CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsArchived { get; private set; }

    private EmployeeNote() { }

    public static EmployeeNote Create(
        EmployeeNoteId id,
        EmployeeId employeeId,
        string content,
        string createdByUserId,
        NoteCategory? category = null)
    {
        return new EmployeeNote
        {
            Id = id,
            EmployeeId = employeeId,
            Content = content,
            NoteCategory = category ?? NoteCategory.General,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            IsArchived = false
        };
    }

    public void Archive()
    {
        if (IsArchived) return;
        IsArchived = true;
    }
}
