using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.BulkImport;

public sealed class BulkImportJob : Entity<BulkImportJobId>
{
    public string EntityType { get; set; }
    public string OriginalFileName { get; set; }
    public string StatusCode { get; set; }
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int FailedRows { get; set; }

    /// <summary>JSON — parsed + validated preview data. No raw file bytes stored.</summary>
    public string PreviewDataJson { get; set; }

    /// <summary>JSON — per-row error details for error file generation.</summary>
    public string? ErrorDetails { get; set; }

    public string? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long ExecutionDurationMs { get; set; }
}
