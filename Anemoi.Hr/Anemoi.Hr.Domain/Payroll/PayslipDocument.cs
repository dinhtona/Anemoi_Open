using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Domain.Payroll;

public sealed class PayslipDocument : Entity<PayslipDocumentId>
{
    public PayslipId PayslipId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public string StoragePath { get; private set; } = default!;
    public long FileSize { get; private set; }
    public string ChecksumHash { get; private set; } = default!;
    public string GeneratedBy { get; private set; } = default!;
    public DateTime GeneratedAt { get; private set; }
    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Payslip Payslip { get; private set; } = default!;

    private PayslipDocument() { }

    public static PayslipDocument Create(
        PayslipId payslipId,
        string fileName,
        string contentType,
        string storagePath,
        long fileSize,
        string checksumHash,
        string generatedBy,
        DateTime generatedAt,
        int version)
    {
        return new PayslipDocument
        {
            Id = new PayslipDocumentId(Guid.NewGuid()),
            PayslipId = payslipId,
            FileName = fileName,
            ContentType = contentType,
            StoragePath = storagePath,
            FileSize = fileSize,
            ChecksumHash = checksumHash,
            GeneratedBy = generatedBy,
            GeneratedAt = generatedAt,
            Version = version,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
