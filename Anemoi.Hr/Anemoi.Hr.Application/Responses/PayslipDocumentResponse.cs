using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class PayslipDocumentResponse
{
    public Guid Id { get; set; }
    public Guid PayslipId { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public string StoragePath { get; set; } = default!;
    public long FileSize { get; set; }
    public string ChecksumHash { get; set; } = default!;
    public string GeneratedBy { get; set; } = default!;
    public DateTime GeneratedAt { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
}
