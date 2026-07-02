using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.EmployeeDocuments;

public sealed class EmployeeDocument : Entity<EmployeeDocumentId>
{
    public EmployeeId EmployeeId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DisplayName { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? IssuedBy { get; private set; }
    public DateOnly? IssuedDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public string? StorageKey { get; private set; }
    public string? FileName { get; private set; }
    public string? MimeType { get; private set; }
    public long? FileSize { get; private set; }
    public string? Notes { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private EmployeeDocument() { }

    public static EmployeeDocument Create(
        EmployeeDocumentId id,
        EmployeeId employeeId,
        DocumentType documentType,
        string displayName,
        string? referenceNumber = null,
        string? issuedBy = null,
        DateOnly? issuedDate = null,
        DateOnly? expiryDate = null,
        string? storageKey = null,
        string? fileName = null,
        string? mimeType = null,
        long? fileSize = null,
        string? notes = null)
    {
        return new EmployeeDocument
        {
            Id = id,
            EmployeeId = employeeId,
            DocumentType = documentType,
            DisplayName = displayName,
            ReferenceNumber = referenceNumber,
            IssuedBy = issuedBy,
            IssuedDate = issuedDate,
            ExpiryDate = expiryDate,
            StorageKey = storageKey,
            FileName = fileName,
            MimeType = mimeType,
            FileSize = fileSize,
            Notes = notes,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateInfo(
        DocumentType documentType,
        string displayName,
        string? referenceNumber = null,
        string? issuedBy = null,
        DateOnly? issuedDate = null,
        DateOnly? expiryDate = null,
        string? storageKey = null,
        string? fileName = null,
        string? mimeType = null,
        long? fileSize = null,
        string? notes = null)
    {
        DocumentType = documentType;
        DisplayName = displayName;
        ReferenceNumber = referenceNumber;
        IssuedBy = issuedBy;
        IssuedDate = issuedDate;
        ExpiryDate = expiryDate;
        StorageKey = storageKey;
        FileName = fileName;
        MimeType = mimeType;
        FileSize = fileSize;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (IsArchived) return;
        IsArchived = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
