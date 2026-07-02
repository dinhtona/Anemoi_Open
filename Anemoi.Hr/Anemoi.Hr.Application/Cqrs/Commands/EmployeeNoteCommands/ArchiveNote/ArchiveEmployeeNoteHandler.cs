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

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeNoteCommands.ArchiveNote;

public sealed class ArchiveEmployeeNoteHandler(
    ISqlRepository<EmployeeNote> noteRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ArchiveEmployeeNoteCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        ArchiveEmployeeNoteCommand request, CancellationToken ct)
    {
        var note = await noteRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, ct);
        if (note is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNoteNotFound);

        note.Archive();

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = note.EmployeeId,
            EntityType = "EmployeeNote",
            EntityId = request.Id.Value.ToString(),
            EventType = "NoteArchived",
            Title = "Employee note archived",
            Description = note.Content.Length > 200 ? note.Content[..200] : note.Content,
            MetadataJson = JsonSerializer.Serialize(new
            {
                content = note.Content
            }),
            CorrelationId = "",
            OccurredAt = DateTime.UtcNow,
            ActorUserId = Guid.TryParse(request.ArchivedByUserId, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new None();
    }
}
