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

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeDocumentCommands.CreateDocument;

public sealed class CreateEmployeeDocumentHandler(
    ISqlRepository<EmployeeDocument> documentRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEmployeeDocumentCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        CreateEmployeeDocumentCommand request, CancellationToken ct)
    {
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

        var documentId = new EmployeeDocumentId(IdGenerator.NextGuid());
        var now = DateTime.UtcNow;

        var document = EmployeeDocument.Create(
            documentId,
            request.EmployeeId,
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

        await documentRepository.CreateOneAsync(document, ct);

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            EntityType = "EmployeeDocument",
            EntityId = documentId.Value.ToString(),
            EventType = "Created",
            Title = "Employee document created",
            Description = request.DisplayName,
            MetadataJson = "{}",
            CorrelationId = "",
            OccurredAt = now,
            ActorUserId = Guid.TryParse(request.CreatedBy, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new None();
    }
}
