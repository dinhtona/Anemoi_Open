namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeDocumentResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string DocumentType { get; set; }
    public string DisplayName { get; set; }
    public string ReferenceNumber { get; set; }
    public string IssuedBy { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string StorageKey { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public long? FileSize { get; set; }
    public string Notes { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
