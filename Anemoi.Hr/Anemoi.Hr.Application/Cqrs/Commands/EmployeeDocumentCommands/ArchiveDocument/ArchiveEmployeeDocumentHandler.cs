using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.EmployeeDocuments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.ArchiveDocument;

public sealed class ArchiveEmployeeDocumentHandler(
    ISqlRepository<EmployeeDocument> documentRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ArchiveEmployeeDocumentCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        ArchiveEmployeeDocumentCommand request, CancellationToken ct)
    {
        var document = await documentRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, ct);
        if (document is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeDocumentNotFound);

        document.Archive();

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = document.EmployeeId,
            EntityType = "EmployeeDocument",
            EntityId = request.Id.Value.ToString(),
            EventType = "Archived",
            Title = "Employee document archived",
            Description = document.DisplayName,
            OccurredAt = DateTime.UtcNow,
            ActorUserId = Guid.TryParse(request.ArchivedBy, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new None();
    }
}
