using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.EmployeeDocuments;

namespace Anemoi.Hr.Application.Mappings;

public sealed class EmployeeDocumentMapper
{
    public EmployeeDocumentResponse ToResponse(EmployeeDocument document)
    {
        if (document is null) return null;
        return new EmployeeDocumentResponse
        {
            Id = document.Id.Value.ToString(),
            EmployeeId = document.EmployeeId.Value.ToString(),
            DocumentType = document.DocumentType.Value,
            DisplayName = document.DisplayName,
            ReferenceNumber = document.ReferenceNumber,
            IssuedBy = document.IssuedBy,
            IssuedDate = document.IssuedDate,
            ExpiryDate = document.ExpiryDate,
            StorageKey = document.StorageKey,
            FileName = document.FileName,
            MimeType = document.MimeType,
            FileSize = document.FileSize,
            Notes = document.Notes,
            IsArchived = document.IsArchived,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    public IReadOnlyCollection<EmployeeDocumentResponse> ToResponses(IEnumerable<EmployeeDocument> documents)
    {
        if (documents is null) return [];
        return documents.Select(ToResponse).ToList();
    }
}
