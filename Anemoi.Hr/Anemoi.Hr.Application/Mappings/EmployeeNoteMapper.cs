using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeNotes;

namespace Anemoi.Hr.Application.Mappings;

public sealed class EmployeeNoteMapper
{
    public EmployeeNoteResponse ToResponse(EmployeeNote note)
    {
        if (note is null) return null;
        return new EmployeeNoteResponse
        {
            Id = note.Id.Value.ToString(),
            EmployeeId = note.EmployeeId.Value.ToString(),
            Content = note.Content,
            NoteCategory = note.NoteCategory.Value,
            CreatedByUserId = note.CreatedByUserId,
            CreatedAt = note.CreatedAt,
            IsArchived = note.IsArchived
        };
    }

    public IReadOnlyCollection<EmployeeNoteResponse> ToResponses(IEnumerable<EmployeeNote> notes)
    {
        if (notes is null) return [];
        return notes.Select(ToResponse).ToList();
    }
}
