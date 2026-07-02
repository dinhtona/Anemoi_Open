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

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.UpdateDocument;

public sealed class UpdateEmployeeDocumentHandler(
    ISqlRepository<EmployeeDocument> documentRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEmployeeDocumentCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        UpdateEmployeeDocumentCommand request, CancellationToken ct)
    {
        var document = await documentRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, ct);
        if (document is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeDocumentNotFound);

        if (string.IsNullOrWhiteSpace(request.DocumentType))
            return HrErrorResponses.Create(HrBusinessErrorCodes.DocumentTypeRequired);

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return HrErrorResponses.Create(HrBusinessErrorCodes.DisplayNameRequired);

        DocumentType documentType;
        try
        {
            documentType = DocumentType.FromValue(request.DocumentType);
        }
        catch (ArgumentException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.DocumentTypeRequired);
        }

        document.UpdateInfo(
            documentType,
            request.DisplayName,
            request.ReferenceNumber,
            request.IssuedBy,
            request.IssuedDate,
            request.ExpiryDate,
            request.StorageKey,
            request.FileName,
            request.MimeType,
            request.FileSize,
            request.Notes);

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = document.EmployeeId,
            EntityType = "EmployeeDocument",
            EntityId = request.Id.Value.ToString(),
            EventType = "Updated",
            Title = "Employee document updated",
            Description = request.DisplayName,
            OccurredAt = DateTime.UtcNow,
            ActorUserId = Guid.TryParse(request.UpdatedBy, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new None();
    }
}
