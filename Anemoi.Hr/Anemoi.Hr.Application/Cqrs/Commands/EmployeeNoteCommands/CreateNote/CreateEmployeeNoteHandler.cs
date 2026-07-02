using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.EmployeeNotes;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeNoteCommands.CreateNote;

public sealed class CreateEmployeeNoteHandler(
    ISqlRepository<EmployeeNote> noteRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEmployeeNoteCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        CreateEmployeeNoteCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return HrErrorResponses.Create(HrBusinessErrorCodes.ContentRequired);

        NoteCategory noteCategory;
        if (request.NoteCategory is not null)
        {
            try
            {
                noteCategory = NoteCategory.FromValue(request.NoteCategory);
            }
            catch (ArgumentException)
            {
                noteCategory = NoteCategory.General;
            }
        }
        else
        {
            noteCategory = NoteCategory.General;
        }

        var noteId = new EmployeeNoteId(IdGenerator.NextGuid());
        var now = DateTime.UtcNow;

        var note = EmployeeNote.Create(
            noteId,
            request.EmployeeId,
            request.Content,
            request.CreatedByUserId,
            noteCategory);

        await noteRepository.CreateOneAsync(note, ct);

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            EntityType = "EmployeeNote",
            EntityId = noteId.Value.ToString(),
            EventType = "NoteCreated",
            Title = "Employee note created",
            Description = request.Content.Length > 200 ? request.Content[..200] : request.Content,
            MetadataJson = JsonSerializer.Serialize(new
            {
                content = request.Content,
                noteCategory = noteCategory.Value
            }),
            CorrelationId = "",
            OccurredAt = now,
            ActorUserId = Guid.TryParse(request.CreatedByUserId, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new None();
    }
}
