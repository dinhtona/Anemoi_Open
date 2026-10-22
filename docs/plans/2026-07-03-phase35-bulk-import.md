# Phase 35 — Bulk Import Framework Implementation Plan (Revised)

> **For agentic workers:** Inline execution, step by step.

**Goal:** Generic Bulk Import Framework (sync-only, single-transaction) with Employee as first implementation.

**Architecture:** Generic pipeline interfaces in `Anemoi.BuildingBlocks` → entity-specific strategies in `Anemoi.Hr`. Upload → Parse → Validate (4 stages) → Preview → Confirm Execute. Single `SaveChanges` ownership. Preview stored as JSON (not raw file bytes). Error files generated on demand. BulkImportJob is an audit record.

**Tech Stack:** .NET 10, Clean Architecture, CQRS/MediatR, EPPlus (existing), Mapperly, FluentValidation, OneOf, EF Core/PostgreSQL.

## Key Design Decisions (post-review)

1. **No UploadedFileBytes (bytea) storage** — Upload parses + validates, stores validated rows as `ImportPreviewData` (JSON) in `BulkImportJob.PreviewData`.
2. **Single parse** — Upload produces preview. Execute consumes `PreviewData` directly (no re-parse).
3. **ExecuteImportHandler is thin** — delegates to `IImportOrchestrator`. Handler: load job → call orchestrator → return.
4. **Employee creation uses existing project pattern**: The codebase uniformly uses `new Employee { ... }` object initializer (no factory, no static create method). See `CreateEmployeeHandler.cs` and `ConvertCandidateToEmployeeHandler.cs`. The import mapper follows the same established pattern.
5. **4 validation stages**: Syntax → Business → Duplicate → Reference
6. **Duplicate detection**: in-file (code, email) + in-database (code, email)
7. **Single SaveChanges — implicit EF Core transaction**: `IUnitOfWork` has only `SaveChangesAsync` (no `BeginTransactionAsync`). EF Core wraps each SaveChanges in an implicit transaction. Orchestrator: (a) if validation fails → update job status only → SaveChanges; (b) if all valid → add employees + history + update job → single SaveChanges persists everything atomically; (c) if handler throws → no SaveChanges (job stays "Pending", frontend handles stale state).
8. **BulkImportJob is an audit record**: Not a rich aggregate. Simple properties, JSON preview data, no business methods.
9. **Error files generated from PreviewData**: `IBulkImportErrorFileGenerator` receives `ImportPreviewData` + columns (not original file bytes). Builds xlsx from stored row data + re-validated errors.
10. **ADR-035**: Documented architecture decision.

## File Structure

### BuildingBlocks (Generic Framework)
```
Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/
├── Abstractions/
│   ├── IBulkImportHandler.cs          ← Generic: process validated rows
│   ├── IBulkImportRowValidator.cs     ← Generic: validate one row
│   ├── IBulkImportTemplateProvider.cs ← Generic: column definitions
│   └── IBulkImportErrorFileGenerator.cs ← Generic: build error xlsx
├── Models/
│   ├── BulkImportRowResult.cs
│   ├── BulkImportValidationError.cs
│   ├── BulkImportPreview.cs
│   ├── BulkImportResult.cs
│   └── BulkImportConstants.cs
├── Services/
│   ├── BulkExcelParserService.cs      ← Parse xlsx/csv → List<Dictionary>
│   └── BulkExcelTemplateService.cs    ← Generate template xlsx + error xlsx
```

### Hr Domain
```
Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/BulkImportJobId.cs
Anemoi.Hr/Anemoi.Hr.Domain/BulkImport/
├── BulkImportJob.cs         ← Audit record (not rich aggregate)
├── BulkImportJobStatus.cs   ← Status constants
├── BulkImportJobError.cs    ← Value object for row errors
└── ImportPreviewData.cs     ← Stored as JSON: parsed rows + validation results
```

### Hr Application
```
Anemoi.Hr/Anemoi.Hr.Application/BulkImport/
├── Abstractions/
│   └── IImportOrchestrator.cs          ← Single orchestration interface for execute
├── Models/
│   └── ImportedEmployeeResult.cs       ← Contains created IDs
├── EmployeeImport/
│   ├── EmployeeImportRowDto.cs
│   ├── Validation/
│   │   ├── SyntaxValidator.cs          ← Stage 1: format, types, required
│   │   ├── BusinessValidator.cs        ← Stage 2: status transitions, enum values
│   │   ├── DuplicateValidator.cs       ← Stage 3: in-file + in-db
│   │   └── ReferenceValidator.cs       ← Stage 4: department, position, manager
│   ├── EmployeeImportHandler.cs        ← IBulkImportHandler — creates entities (no save)
│   ├── EmployeeImportOrchestrator.cs   ← IImportOrchestrator — owns SaveChanges
│   ├── EmployeeImportMappingService.cs ← Row → Employee using domain patterns
│   ├── EmployeeImportTemplateProvider.cs
│   └── EmployeeImportErrorFileGenerator.cs
├── Cqrs/Commands/EmployeeImport/
│   ├── UploadImportFileCommand.cs + Handler
│   ├── PreviewImportCommand.cs + Handler
│   ├── ExecuteImportCommand.cs + Handler
│   └── EmployeeImportValidators.cs
├── Cqrs/Queries/EmployeeImport/
│   ├── GetImportHistoryQuery.cs + Handler
│   ├── GetImportJobDetailQuery.cs + Handler
│   ├── GetImportTemplateQuery.cs + Handler
│   └── DownloadErrorFileQuery.cs + Handler
└── Responses/
    ├── EmployeeImportPreviewResponse.cs
    ├── EmployeeImportResultResponse.cs
    ├── EmployeeImportHistoryResponse.cs
    ├── EmployeeImportJobDetailResponse.cs
    └── FileResponse.cs
```

### Hr Infrastructure
```
Anemoi.Hr/Anemoi.Hr.Infrastructure/Persistence/EntityConfigurations/BulkImportJobConfiguration.cs
```

### Hr API
```
Anemoi.Hr/Anemoi.Hr.Api/Controllers/Employee/EmployeeImportController.cs
```

### Tests
```
Anemoi.Hr/Anemoi.Hr.Test/BulkImport/
├── Validation/
│   ├── SyntaxValidatorTests.cs
│   ├── BusinessValidatorTests.cs
│   ├── DuplicateValidatorTests.cs
│   └── ReferenceValidatorTests.cs
├── Services/
│   ├── BulkExcelParserServiceTests.cs
│   ├── BulkExcelTemplateServiceTests.cs
│   └── EmployeeImportMappingServiceTests.cs
├── Handlers/
│   ├── EmployeeImportHandlerTests.cs
│   ├── UploadImportFileHandlerTests.cs
│   ├── PreviewImportHandlerTests.cs
│   └── ExecuteImportHandlerTests.cs
└── Integration/
    ├── UploadPreviewExecuteIntegrationTests.cs
    └── ImportHistoryIntegrationTests.cs
```

### Frontend
```
cody-web-app/src/
├── types/hr/bulk-import.ts
├── services/hr/bulk-import.ts
├── hooks/hr/use-bulk-import.ts
├── components/features/hr/import/
│   ├── import-wizard.tsx
│   ├── step-upload.tsx
│   ├── step-validate.tsx
│   ├── step-preview.tsx
│   ├── step-result.tsx
│   └── import-history-dialog.tsx
├── app/[locale]/(dashboard)/hr/employees/import/
│   ├── page.tsx
│   └── layout.tsx
└── messages/{vi,en}.json  (additions)
```

---

## Task 1: Generic Interfaces & Models (BuildingBlocks)

**Files:**
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Abstractions/IBulkImportHandler.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Abstractions/IBulkImportRowValidator.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Abstractions/IBulkImportTemplateProvider.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Abstractions/IBulkImportErrorFileGenerator.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Models/BulkImportRowResult.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Models/BulkImportValidationError.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Models/BulkImportPreview.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Models/BulkImportResult.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Models/BulkImportConstants.cs`

- [ ] **1.1 — IBulkImportHandler.cs**

```csharp
using OneOf;

namespace Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

/// <summary>
/// Processes validated rows into domain entities in memory.
/// Does NOT call SaveChanges — the caller owns the transaction.
/// Returns created entity IDs or per-row errors.
/// </summary>
public interface IBulkImportHandler<in TRow>
{
    Task<OneOf<IReadOnlyCollection<BulkImportRowResult>, ErrorDetailResponse>> HandleAsync(
        IReadOnlyCollection<TRow> validRows,
        string actorUserId,
        CancellationToken ct);
}
```

- [ ] **1.2 — IBulkImportRowValidator.cs**

```csharp
namespace Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

/// <summary>
/// Validates a single row DTO. Returns 0..N errors per row.
/// </summary>
public interface IBulkImportRowValidator<in TRow>
{
    Task<IReadOnlyCollection<BulkImportValidationError>> ValidateAsync(
        TRow row, int rowIndex, CancellationToken ct);
}
```

- [ ] **1.3 — IBulkImportTemplateProvider.cs**

```csharp
namespace Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

public interface IBulkImportTemplateProvider
{
    string TemplateFileName { get; }
    IReadOnlyCollection<ImportColumnDefinition> Columns { get; }
}

public sealed record ImportColumnDefinition(
    string Header,
    string BusinessName,
    bool IsRequired,
    string? ExampleValue,
    string? ValidationNote);
```

- [ ] **1.4 — IBulkImportErrorFileGenerator.cs**

```csharp
using Anemoi.BuildingBlock.Application.BulkImport.Models;

namespace Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

/// <summary>
/// Generates an error xlsx from preview data + per-row results.
/// Does NOT require original file bytes — builds from stored data + columns.
/// </summary>
public interface IBulkImportErrorFileGenerator
{
    byte[] GenerateErrorFile(
        IReadOnlyCollection<ImportPreviewRow> previewRows,
        IReadOnlyCollection<BulkImportRowResult> rowResults,
        IReadOnlyCollection<ImportColumnDefinition> columns);
}
```

- [ ] **1.5 — BulkImportValidationError.cs**

```csharp
namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record BulkImportValidationError(
    int RowIndex,
    string Column,
    string Value,
    string ErrorMessage,
    string Severity); // "Error" | "Warning"
```

- [ ] **1.6 — BulkImportRowResult.cs**

```csharp
namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record BulkImportRowResult(
    int RowIndex,
    BulkImportRowStatus Status,
    string? EntityId,
    IReadOnlyCollection<BulkImportValidationError>? Errors);

public enum BulkImportRowStatus
{
    Success,
    Skipped,
    Failed
}
```

- [ ] **1.7 — BulkImportPreview.cs**

```csharp
namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record BulkImportPreview(
    int TotalRows,
    int ValidRows,
    int WarningCount,
    int ErrorCount,
    IReadOnlyCollection<BulkImportValidationError> AllErrors,
    IReadOnlyCollection<BulkImportRowResult> RowResults);
```

- [ ] **1.8 — BulkImportResult.cs**

```csharp
namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public sealed record BulkImportResult(
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    IReadOnlyCollection<BulkImportRowResult> RowResults);
```

- [ ] **1.9 — BulkImportConstants.cs**

```csharp
namespace Anemoi.BuildingBlock.Application.BulkImport.Models;

public static class BulkImportConstants
{
    public const int MaxUploadRows = 10000;
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string CsvContentType = "text/csv";
}
```

- [ ] **1.10 — Verify compile**: `dotnet build Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/`

---

## Task 2: Excel Services (BuildingBlocks)

**Files:**
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Services/BulkExcelParserService.cs`
- Create: `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/BulkImport/Services/BulkExcelTemplateService.cs`

- [ ] **2.1 — BulkExcelParserService.cs**

```csharp
using OfficeOpenXml;

namespace Anemoi.BuildingBlock.Application.BulkImport.Services;

public sealed class BulkExcelParserService
{
    public IReadOnlyCollection<Dictionary<string, string>> Parse(byte[] fileBytes, string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".xlsx" => ParseXlsx(fileBytes),
            ".csv" => ParseCsv(fileBytes),
            _ => throw new ArgumentException($"Unsupported format: {ext}")
        };
    }

    private static List<Dictionary<string, string>> ParseXlsx(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var pkg = new ExcelPackage(stream);
        var ws = pkg.Workbook.Worksheets[0];
        var rowCount = ws.Dimension?.Rows ?? 0;
        var colCount = ws.Dimension?.Columns ?? 0;
        if (rowCount < 2) return [];

        var headers = new List<string>();
        for (var c = 1; c <= colCount; c++)
        {
            var h = ws.Cells[1, c].Text?.Trim();
            if (!string.IsNullOrEmpty(h)) headers.Add(h);
        }

        var result = new List<Dictionary<string, string>>(rowCount - 1);
        for (var r = 2; r <= rowCount; r++)
        {
            var row = new Dictionary<string, string>(headers.Count);
            var empty = true;
            for (var c = 0; c < headers.Count; c++)
            {
                var val = ws.Cells[r, c + 1].Text?.Trim() ?? "";
                row[headers[c]] = val;
                if (val.Length > 0) empty = false;
            }
            if (!empty) result.Add(row);
        }
        return result;
    }

    private static List<Dictionary<string, string>> ParseCsv(byte[] bytes)
    {
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        var lines = text.Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => l.Length > 0)
            .ToList();
        if (lines.Count < 2) return [];

        var headers = SplitCsvLine(lines[0])
            .Select(h => h.Trim('"', ' '))
            .ToList();

        var result = new List<Dictionary<string, string>>(lines.Count - 1);
        for (var i = 1; i < lines.Count; i++)
        {
            var values = SplitCsvLine(lines[i]);
            var row = new Dictionary<string, string>(headers.Count);
            var empty = true;
            for (var c = 0; c < headers.Count && c < values.Count; c++)
            {
                row[headers[c]] = values[c].Trim('"', ' ');
                if (values[c].Length > 0) empty = false;
            }
            if (!empty) result.Add(row);
        }
        return result;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var cur = new System.Text.StringBuilder();
        var inQ = false;
        foreach (var ch in line)
        {
            if (ch == '"') inQ = !inQ;
            else if (ch == ',' && !inQ) { result.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(ch);
        }
        result.Add(cur.ToString());
        return result;
    }
}
```

- [ ] **2.2 — BulkExcelTemplateService.cs**

```csharp
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Anemoi.BuildingBlock.Application.BulkImport.Services;

public sealed class BulkExcelTemplateService
{
    public byte[] GenerateTemplate(IReadOnlyCollection<ImportColumnDefinition> columns)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Template");

        using var hr = ws.Cells[1, 1, 1, columns.Count];
        hr.Style.Font.Bold = true;
        hr.Style.Fill.PatternType = ExcelFillStyle.Solid;
        hr.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
        hr.Style.Font.Color.SetColor(Color.White);

        var i = 0;
        foreach (var col in columns)
        {
            var cell = ws.Cells[1, ++i];
            cell.Value = col.Header;
            if (!string.IsNullOrEmpty(col.ValidationNote))
                ws.Comments.Add(cell, col.ValidationNote, "System");
            if (!string.IsNullOrEmpty(col.ExampleValue))
            {
                ws.Cells[2, i].Value = col.ExampleValue;
                if (col.IsRequired)
                    ws.Cells[2, i].Style.Font.Color.SetColor(Color.Red);
            }
            ws.Column(i).AutoFit();
        }
        ws.View.FreezePanes(2, 1);
        return pkg.GetAsByteArray();
    }

    /// <summary>
    /// Builds error xlsx from preview rows + validation results.
    /// Does NOT require the original uploaded file bytes — constructs new xlsx
    /// from the stored preview data and re-validated errors.
    /// </summary>
    public byte[] GenerateErrorFile(
        IReadOnlyCollection<ImportPreviewRow> previewRows,
        IReadOnlyCollection<BulkImportRowResult> rowResults,
        IReadOnlyCollection<ImportColumnDefinition> columns)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Errors");
        var colList = columns.ToList();

        // Header
        for (var c = 0; c < colList.Count; c++)
            ws.Cells[1, c + 1].Value = colList[c].Header;
        var errCol = colList.Count + 1;
        ws.Cells[1, errCol].Value = "Error Message";
        ws.Cells[1, errCol].Style.Font.Bold = true;
        ws.Cells[1, errCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells[1, errCol].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 0, 0));
        ws.Cells[1, errCol].Style.Font.Color.SetColor(Color.White);

        // Map errors by row index
        var errMap = rowResults
            .Where(r => r.Errors?.Count > 0)
            .ToDictionary(r => r.RowIndex, r => r.Errors);

        // Data rows from preview
        for (var i = 0; i < previewRows.Count; i++)
        {
            var row = previewRows.ElementAt(i);
            for (var c = 0; c < colList.Count; c++)
                ws.Cells[i + 2, c + 1].Value =
                    row.Data.GetValueOrDefault(colList[c].BusinessName, "");

            if (errMap.TryGetValue(i, out var errors))
            {
                ws.Cells[i + 2, errCol].Value = string.Join("; ",
                    errors.Select(e => $"[{e.Severity}] {e.Column}: {e.ErrorMessage}"));
                ws.Cells[i + 2, errCol].Style.Font.Color.SetColor(Color.Red);
            }
        }

        ws.Column(errCol).AutoFit();
        for (var c = 1; c <= colList.Count; c++) ws.Column(c).AutoFit();
        return pkg.GetAsByteArray();
    }
}
```

- [ ] **2.3 — Verify compile**: `dotnet build Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/`

---

## Task 3: Import Job Domain (Hr — Audit Record)

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.ModelIds/ModelIds/BulkImportJobId.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/BulkImport/BulkImportJob.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/BulkImport/BulkImportJobStatus.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/BulkImport/BulkImportJobError.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Domain/BulkImport/ImportPreviewData.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Persistence/EntityConfigurations/BulkImportJobConfiguration.cs`
- Modify: `Anemoi.Hr/Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs`

- [ ] **3.1 — BulkImportJobId.cs**

```csharp
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record BulkImportJobId(Guid Value) : StronglyTypedGuidId<BulkImportJobId>(Value)
{
    public override string ToString() => Value.ToString();
}
```

- [ ] **3.2 — BulkImportJobStatus.cs** (constants only)

```csharp
namespace Anemoi.Hr.Domain.BulkImport;

public static class BulkImportJobStatus
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}
```

- [ ] **3.3 — BulkImportJobError.cs** (value object)

```csharp
namespace Anemoi.Hr.Domain.BulkImport;

public sealed record BulkImportJobError(
    int RowIndex,
    string Column,
    string Value,
    string ErrorMessage,
    string Severity);
```

- [ ] **3.4 — ImportPreviewData.cs** (JSON-stored preview model)

```csharp
namespace Anemoi.Hr.Domain.BulkImport;

/// <summary>
/// Serialized as JSON in BulkImportJob.PreviewData.
/// Contains parsed rows + validation results — no raw file bytes.
/// </summary>
public sealed record ImportPreviewData(
    string OriginalFileName,
    int TotalRows,
    int ValidRows,
    int WarningCount,
    int ErrorCount,
    List<ImportPreviewRow> Rows,
    List<BulkImportJobError> AllErrors);

public sealed record ImportPreviewRow(
    int RowIndex,
    Dictionary<string, string> Data,
    string ValidationStatus, // "Valid" | "Warning" | "Error"
    List<BulkImportJobError>? Errors);
```

- [ ] **3.5 — BulkImportJob.cs** (audit record, not rich aggregate)

```csharp
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

    /// <summary>JSON — parsed + validated preview data. No raw file bytes.</summary>
    public string PreviewDataJson { get; set; }

    /// <summary>JSON — per-row errors for error file generation.</summary>
    public string? ErrorDetails { get; set; }

    public string? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long ExecutionDurationMs { get; set; }
}
```

- [ ] **3.6 — BulkImportJobConfiguration.cs**

```csharp
using Anemoi.Hr.Domain.BulkImport;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Persistence.EntityConfigurations;

public sealed class BulkImportJobConfiguration : IEntityTypeConfiguration<BulkImportJob>
{
    public void Configure(EntityTypeBuilder<BulkImportJob> builder)
    {
        builder.ToTable("bulk_import_jobs", "hr");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, v => new BulkImportJobId(v))
            .ValueGeneratedNever();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ActorUserId).HasMaxLength(100);
        builder.Property(x => x.ActorName).HasMaxLength(255);
        builder.Property(x => x.PreviewDataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ErrorDetails).HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.StatusCode);
    }
}
```

- [ ] **3.7 — Register in HrDbContext**

```csharp
public DbSet<BulkImportJob> BulkImportJobs { get; set; }
// In OnModelCreating:
builder.ApplyConfiguration(new BulkImportJobConfiguration());
```

- [ ] **3.8 — Verify compile**: `dotnet build Anemoi.Hr/Anemoi.Hr.Domain/`

---

## Task 4: Employee Import Validation (4 Stages)

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/EmployeeImportRowDto.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/Validation/SyntaxValidator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/Validation/BusinessValidator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/Validation/DuplicateValidator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/Validation/ReferenceValidator.cs`

- [ ] **4.1 — EmployeeImportRowDto.cs**

```csharp
namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed record EmployeeImportRowDto
{
    public Dictionary<string, string> RawData { get; init; } = new();

    // Business-mapped convenience properties
    public string EmployeeCode => RawData.GetValueOrDefault("EmployeeCode", "");
    public string FirstName => RawData.GetValueOrDefault("FirstName", "");
    public string LastName => RawData.GetValueOrDefault("LastName", "");
    public string WorkEmail => RawData.GetValueOrDefault("WorkEmail", "");
    public string? PersonalEmail => RawData.GetValueOrDefault("PersonalEmail");
    public string? Phone => RawData.GetValueOrDefault("Phone");
    public string? DepartmentName => RawData.GetValueOrDefault("Department");
    public string? PositionName => RawData.GetValueOrDefault("Position");
    public string? GradeName => RawData.GetValueOrDefault("Grade");
    public string? ManagerCode => RawData.GetValueOrDefault("Manager");
    public string? HireDateStr => RawData.GetValueOrDefault("HireDate");
    public string? EmploymentType => RawData.GetValueOrDefault("EmploymentType");
    public string? StatusValue => RawData.GetValueOrDefault("Status");
    public string? DateOfBirthStr => RawData.GetValueOrDefault("DOB");
    public string? IdentityUserEmail => RawData.GetValueOrDefault("IdentityUser");
}
```

- [ ] **4.2 — SyntaxValidator.cs** (Stage 1: format, types, required fields)

```csharp
using Anemoi.BuildingBlock.Application.BulkImport.Models;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;

public sealed class SyntaxValidator
{
    public Task<IReadOnlyCollection<BulkImportValidationError>> ValidateAsync(
        EmployeeImportRowDto row, int rowIndex, CancellationToken ct)
    {
        var errors = new List<BulkImportValidationError>();

        // Required fields
        if (string.IsNullOrWhiteSpace(row.EmployeeCode))
            errors.Add(Error(rowIndex, "EmployeeCode", row.EmployeeCode, "EmployeeCode is required"));
        if (string.IsNullOrWhiteSpace(row.FirstName))
            errors.Add(Error(rowIndex, "FirstName", row.FirstName, "FirstName is required"));
        if (string.IsNullOrWhiteSpace(row.LastName))
            errors.Add(Error(rowIndex, "LastName", row.LastName, "LastName is required"));
        if (string.IsNullOrWhiteSpace(row.WorkEmail))
            errors.Add(Error(rowIndex, "WorkEmail", row.WorkEmail, "WorkEmail is required"));
        if (string.IsNullOrWhiteSpace(row.DepartmentName))
            errors.Add(Error(rowIndex, "Department", row.DepartmentName ?? "", "Department is required"));
        if (string.IsNullOrWhiteSpace(row.PositionName))
            errors.Add(Error(rowIndex, "Position", row.PositionName ?? "", "Position is required"));

        // Email format
        if (!string.IsNullOrWhiteSpace(row.WorkEmail) && !row.WorkEmail.Contains('@'))
            errors.Add(Error(rowIndex, "WorkEmail", row.WorkEmail, "WorkEmail must contain @"));

        // Date formats
        if (!string.IsNullOrWhiteSpace(row.HireDateStr) && !DateOnly.TryParse(row.HireDateStr, out _))
            errors.Add(Error(rowIndex, "HireDate", row.HireDateStr, "HireDate must be yyyy-MM-dd"));

        if (!string.IsNullOrWhiteSpace(row.DateOfBirthStr) && !DateOnly.TryParse(row.DateOfBirthStr, out _))
            errors.Add(Error(rowIndex, "DOB", row.DateOfBirthStr, "DOB must be yyyy-MM-dd"));

        return Task.FromResult<IReadOnlyCollection<BulkImportValidationError>>(errors);
    }

    private static BulkImportValidationError Error(int row, string col, string val, string msg) =>
        new(row, col, val, msg, "Error");
}
```

- [ ] **4.3 — BusinessValidator.cs** (Stage 2: valid enum values, domain knowledge)

```csharp
using Anemoi.BuildingBlock.Application.BulkImport.Models;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;

public sealed class BusinessValidator
{
    private static readonly HashSet<string> ValidEmploymentTypes =
        ["FullTime", "PartTime", "Contract", "Intern", "Probation"];
    private static readonly HashSet<string> ValidStatuses =
        ["Active", "Onboarding", "Suspended", "Resigned", "Terminated", "Archived", "Draft"];

    public Task<IReadOnlyCollection<BulkImportValidationError>> ValidateAsync(
        EmployeeImportRowDto row, int rowIndex, CancellationToken ct)
    {
        var errors = new List<BulkImportValidationError>();

        if (!string.IsNullOrWhiteSpace(row.EmploymentType) &&
            !ValidEmploymentTypes.Contains(row.EmploymentType))
            errors.Add(new BulkImportValidationError(rowIndex, "EmploymentType",
                row.EmploymentType, $"EmploymentType must be: {string.Join(", ", ValidEmploymentTypes)}", "Error"));

        if (!string.IsNullOrWhiteSpace(row.StatusValue) &&
            !ValidStatuses.Contains(row.StatusValue))
            errors.Add(new BulkImportValidationError(rowIndex, "Status",
                row.StatusValue, $"Status must be: {string.Join(", ", ValidStatuses)}", "Warning"));

        return Task.FromResult<IReadOnlyCollection<BulkImportValidationError>>(errors);
    }
}
```

- [ ] **4.4 — DuplicateValidator.cs** (Stage 3: in-file + in-database)

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;

public sealed class DuplicateValidator
{
    private readonly ISqlRepository<Employee> _employeeRepo;

    public DuplicateValidator(ISqlRepository<Employee> employeeRepo)
    {
        _employeeRepo = employeeRepo;
    }

    /// <summary>
    /// Validates duplicates within the file batch.
    /// </summary>
    public IReadOnlyCollection<BulkImportValidationError> ValidateInFile(
        IReadOnlyCollection<EmployeeImportRowDto> rows)
    {
        var errors = new List<BulkImportValidationError>();

        // EmployeeCode duplicates
        var codeGroups = rows
            .Select((r, i) => (Row: r, Index: i))
            .Where(x => !string.IsNullOrWhiteSpace(x.Row.EmployeeCode))
            .GroupBy(x => x.Row.EmployeeCode)
            .Where(g => g.Count() > 1);

        foreach (var group in codeGroups)
            foreach (var dup in group.Skip(1))
                errors.Add(new BulkImportValidationError(dup.Index, "EmployeeCode",
                    dup.Row.EmployeeCode, "Duplicate EmployeeCode within file", "Error"));

        // WorkEmail duplicates
        var emailGroups = rows
            .Select((r, i) => (Row: r, Index: i))
            .Where(x => !string.IsNullOrWhiteSpace(x.Row.WorkEmail))
            .GroupBy(x => x.Row.WorkEmail)
            .Where(g => g.Count() > 1);

        foreach (var group in emailGroups)
            foreach (var dup in group.Skip(1))
                errors.Add(new BulkImportValidationError(dup.Index, "WorkEmail",
                    dup.Row.WorkEmail, "Duplicate WorkEmail within file", "Error"));

        return errors;
    }

    /// <summary>
    /// Validates against existing database records.
    /// </summary>
    public async Task<IReadOnlyCollection<BulkImportValidationError>> ValidateAgainstDatabaseAsync(
        IReadOnlyCollection<EmployeeImportRowDto> rows,
        CancellationToken ct)
    {
        var errors = new List<BulkImportValidationError>();

        var codes = rows.Where(r => !string.IsNullOrWhiteSpace(r.EmployeeCode))
            .Select(r => r.EmployeeCode).Distinct().ToList();
        var emails = rows.Where(r => !string.IsNullOrWhiteSpace(r.WorkEmail))
            .Select(r => r.WorkEmail).Distinct().ToList();

        if (codes.Count > 0)
        {
            var existingCodes = await _employeeRepo.GetQueryable()
                .Where(e => codes.Contains(e.EmployeeCode))
                .Select(e => e.EmployeeCode)
                .ToListAsync(ct);
            var codeSet = existingCodes.ToHashSet();

            for (var i = 0; i < rows.Count; i++)
            {
                if (codeSet.Contains(rows[i].EmployeeCode))
                    errors.Add(new BulkImportValidationError(i, "EmployeeCode",
                        rows[i].EmployeeCode, "EmployeeCode already exists in system", "Error"));
            }
        }

        if (emails.Count > 0)
        {
            var existingEmails = await _employeeRepo.GetQueryable()
                .Where(e => emails.Contains(e.WorkEmail))
                .Select(e => e.WorkEmail)
                .ToListAsync(ct);
            var emailSet = existingEmails.ToHashSet();

            for (var i = 0; i < rows.Count; i++)
            {
                if (emailSet.Contains(rows[i].WorkEmail))
                    errors.Add(new BulkImportValidationError(i, "WorkEmail",
                        rows[i].WorkEmail, "WorkEmail already exists in system", "Error"));
            }
        }

        return errors;
    }
}
```

- [ ] **4.5 — ReferenceValidator.cs** (Stage 4: department, position, manager exist)

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;

public sealed class ReferenceValidator
{
    private readonly ISqlRepository<Department> _deptRepo;
    private readonly ISqlRepository<Position> _posRepo;
    private readonly ISqlRepository<Employee> _empRepo;

    public ReferenceValidator(
        ISqlRepository<Department> deptRepo,
        ISqlRepository<Position> posRepo,
        ISqlRepository<Employee> empRepo)
    {
        _deptRepo = deptRepo;
        _posRepo = posRepo;
        _empRepo = empRepo;
    }

    public async Task<(IReadOnlyCollection<BulkImportValidationError> Errors,
        Dictionary<string, Department> Departments,
        Dictionary<string, Position> Positions,
        Dictionary<string, Employee> Managers)>
        ValidateAndResolveAsync(IReadOnlyCollection<EmployeeImportRowDto> rows, CancellationToken ct)
    {
        var errors = new List<BulkImportValidationError>();

        // Resolve departments
        var deptNames = rows.Where(r => r.DepartmentName != null)
            .Select(r => r.DepartmentName!).Distinct().ToList();
        var departments = await _deptRepo.GetQueryable()
            .Where(d => deptNames.Contains(d.Name))
            .ToDictionaryAsync(d => d.Name, ct);

        // Resolve positions
        var posNames = rows.Where(r => r.PositionName != null)
            .Select(r => r.PositionName!).Distinct().ToList();
        var positions = await _posRepo.GetQueryable()
            .Where(p => posNames.Contains(p.Name))
            .ToDictionaryAsync(p => p.Name, ct);

        // Resolve managers
        var mgrCodes = rows.Where(r => r.ManagerCode != null)
            .Select(r => r.ManagerCode!).Distinct().ToList();
        var managers = await _empRepo.GetQueryable()
            .Where(m => mgrCodes.Contains(m.EmployeeCode))
            .ToDictionaryAsync(m => m.EmployeeCode, ct);

        // Validate each row
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];

            if (row.DepartmentName != null && !departments.ContainsKey(row.DepartmentName))
                errors.Add(new BulkImportValidationError(i, "Department", row.DepartmentName,
                    $"Department '{row.DepartmentName}' not found", "Error"));

            if (row.PositionName != null && !positions.ContainsKey(row.PositionName))
                errors.Add(new BulkImportValidationError(i, "Position", row.PositionName,
                    $"Position '{row.PositionName}' not found", "Error"));

            if (row.ManagerCode != null && !managers.ContainsKey(row.ManagerCode))
                errors.Add(new BulkImportValidationError(i, "Manager", row.ManagerCode,
                    $"Manager with code '{row.ManagerCode}' not found", "Warning"));
        }

        return (errors, departments, positions, managers);
    }
}
```

- [ ] **4.6 — Verify compile**: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/`

---

## Task 5: Employee Import Handler & Orchestrator

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/Abstractions/IImportOrchestrator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/Models/ImportedEmployeeResult.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/EmployeeImportMappingService.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/EmployeeImportHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/EmployeeImportOrchestrator.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/EmployeeImportTemplateProvider.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/BulkImport/EmployeeImport/EmployeeImportErrorFileGenerator.cs`

- [ ] **5.1 — IImportOrchestrator.cs**

```csharp
using OneOf;

namespace Anemoi.Hr.Application.BulkImport.Abstractions;

/// <summary>
/// Single orchestrator for import execution. Owns SaveChanges.
/// Implementations handle: validate → execute → persist → update job.
/// </summary>
public interface IImportOrchestrator
{
    Task<OneOf<EmployeeImportResultResponse, ErrorDetailResponse>> ExecuteImportAsync(
        Domain.BulkImport.BulkImportJob job,
        string actorUserId,
        string? actorName,
        CancellationToken ct);
}
```

- [ ] **5.2 — ImportedEmployeeResult.cs**

```csharp
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.BulkImport.Models;

public sealed record ImportedEmployeeResult(
    EmployeeId EmployeeId,
    int RowIndex);
```

- [ ] **5.3 — EmployeeImportMappingService.cs** (follows existing project pattern — see `CreateEmployeeHandler.cs` lines 57-78 which use `new Employee { ... }` object initializer. No factory exists in this codebase. The same pattern is used in `ConvertCandidateToEmployeeHandler.cs` and `HrDevSeedData.cs`.)

```csharp
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportMappingService
{
    public Employee MapToEmployee(
        EmployeeImportRowDto row,
        Department department,
        Position position,
        Employee? manager)
    {
        var now = DateTime.UtcNow;
        var employeeId = new EmployeeId(IdGenerator.NextGuid());

        DateOnly? dob = null;
        if (DateOnly.TryParse(row.DateOfBirthStr, out var parsedDob))
            dob = parsedDob;

        var joinDate = DateOnly.FromDateTime(DateTime.UtcNow);
        if (DateOnly.TryParse(row.HireDateStr, out var parsedHire))
            joinDate = parsedHire;

        var statusCode = string.IsNullOrWhiteSpace(row.StatusValue)
            ? EmploymentStatusCode.Draft
            : row.StatusValue;

        // Use property initialization but respect domain invariants
        // The Employee constructor/init pattern matches existing codebase
        return new Employee
        {
            Id = employeeId,
            EmployeeCode = row.EmployeeCode,
            FirstName = row.FirstName,
            LastName = row.LastName,
            WorkEmail = row.WorkEmail,
            PersonalEmail = row.PersonalEmail ?? string.Empty,
            PhoneNumber = row.Phone ?? string.Empty,
            DateOfBirth = dob,
            JoinDate = joinDate,
            EmploymentStatusCode = statusCode,
            EmploymentTypeCode = row.EmploymentType ?? "FullTime",
            GradeCode = row.GradeName,
            PrimaryDepartmentId = department.Id,
            PrimaryPositionId = position.Id,
            DirectManagerEmployeeId = manager?.Id,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public EmployeeHistory MapToHistory(Employee employee, Guid? actorGuid)
    {
        return new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = employee.Id,
            EntityType = "Employee",
            EntityId = employee.Id.Value.ToString(),
            EventType = "BulkImported",
            Title = "Bulk import",
            Description = $"{employee.FullName} ({employee.EmployeeCode})",
            OccurredAt = DateTime.UtcNow,
            ActorUserId = actorGuid
        };
    }
}
```

- [ ] **5.4 — EmployeeImportHandler.cs** (IBulkImportHandler — creates entities, no SaveChanges)

```csharp
using OneOf;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.Hr.Application.BulkImport.Models;
using Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportHandler
{
    private readonly ReferenceValidator _referenceValidator;
    private readonly EmployeeImportMappingService _mappingService;
    private readonly ISqlRepository<Employee> _employeeRepo;
    private readonly ISqlRepository<EmployeeHistory> _historyRepo;

    public EmployeeImportHandler(
        ReferenceValidator referenceValidator,
        EmployeeImportMappingService mappingService,
        ISqlRepository<Employee> employeeRepo,
        ISqlRepository<EmployeeHistory> historyRepo)
    {
        _referenceValidator = referenceValidator;
        _mappingService = mappingService;
        _employeeRepo = employeeRepo;
        _historyRepo = historyRepo;
    }

    public async Task<OneOf<IReadOnlyCollection<BulkImportRowResult>, ErrorDetailResponse>> HandleAsync(
        IReadOnlyCollection<EmployeeImportRowDto> validRows,
        string actorUserId,
        CancellationToken ct)
    {
        // Stage 4: Reference validation + resolution (batched)
        var (refErrors, departments, positions, managers) =
            await _referenceValidator.ValidateAndResolveAsync(validRows, ct);

        if (refErrors.Any(e => e.Severity == "Error"))
        {
            // Return per-row errors instead of failing the entire batch
            var results = validRows.Select((_, i) =>
            {
                var rowErrors = refErrors.Where(e => e.RowIndex == i).ToList();
                return new BulkImportRowResult(i,
                    rowErrors.Count > 0 ? BulkImportRowStatus.Failed : BulkImportRowStatus.Success,
                    null, rowErrors.Count > 0 ? rowErrors : null);
            }).ToList().AsReadOnly();

            return new OneOf<IReadOnlyCollection<BulkImportRowResult>, ErrorDetailResponse>(results);
        }

        var actorGuid = Guid.TryParse(actorUserId, out var g) ? g : null;
        var employees = new List<Employee>(validRows.Count);
        var histories = new List<EmployeeHistory>(validRows.Count);
        var results = new List<BulkImportRowResult>(validRows.Count);

        foreach (var row in validRows)
        {
            var dept = departments.GetValueOrDefault(row.DepartmentName ?? "");
            var pos = positions.GetValueOrDefault(row.PositionName ?? "");
            var mgr = row.ManagerCode != null ? managers.GetValueOrDefault(row.ManagerCode) : null;

            var employee = _mappingService.MapToEmployee(row, dept!, pos!, mgr);
            var history = _mappingService.MapToHistory(employee, actorGuid);

            employees.Add(employee);
            histories.Add(history);
            results.Add(new BulkImportRowResult(0, BulkImportRowStatus.Success,
                employee.Id.Value.ToString(), null));
        }

        await _employeeRepo.CreateManyAsync(employees, ct);
        await _historyRepo.CreateManyAsync(histories, ct);

        return results.AsReadOnly();
    }
}
```

- [ ] **5.5 — EmployeeImportOrchestrator.cs** (owns SaveChanges, single transaction)

```csharp
using OneOf;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.BulkImport.Services;
using Anemoi.Hr.Application.BulkImport.Abstractions;
using Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.BulkImport;
using System.Text.Json;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

/// <summary>
/// Orchestrates import execution. Transaction model:
///
///   1. VALIDATION ERRORS → update job status to Failed → SaveChanges
///      (only the single BulkImportJob row is tracked — safe)
///
///   2. HANDLER SUCCEEDS → entities added to change tracker + job updated →
///      single SaveChanges persists everything atomically (EF Core implicit transaction)
///
///   3. HANDLER THROWS → no SaveChanges → nothing persisted.
///      Job stays "Pending" in DB (stale state handled by frontend).
///      Failure audit is best-effort — separate catch block attempts SaveChanges.
///
/// IUnitOfWork has NO BeginTransactionAsync. EF Core implicit transaction
/// on SaveChangesAsync is the project-wide pattern. No explicit transaction
/// is used — matching existing codebase conventions.
/// </summary>
```

- [ ] **5.6 — EmployeeImportTemplateProvider.cs**

```csharp
using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportTemplateProvider : IBulkImportTemplateProvider
{
    public string TemplateFileName => "employee-import-template.xlsx";

    public IReadOnlyCollection<ImportColumnDefinition> Columns =>
    [
        new("Employee Code", "EmployeeCode", true, "EMP001", "Unique, max 50 chars"),
        new("First Name", "FirstName", true, "John", ""),
        new("Last Name", "LastName", true, "Smith", ""),
        new("Work Email", "WorkEmail", true, "john@company.com", "Must be valid email"),
        new("Personal Email", "PersonalEmail", false, "john@gmail.com", ""),
        new("Phone", "Phone", false, "+84 123 456 789", ""),
        new("Department", "Department", true, "Engineering", "Must match existing department"),
        new("Position", "Position", true, "Software Engineer", "Must match existing position"),
        new("Grade", "Grade", false, "Grade 5", ""),
        new("Manager", "Manager", false, "EMP002", "Existing employee code"),
        new("HireDate", "HireDate", true, "2026-01-15", "Format: yyyy-MM-dd"),
        new("EmploymentType", "EmploymentType", false, "FullTime",
            "FullTime/PartTime/Contract/Intern/Probation"),
        new("Status", "Status", false, "Active",
            "Active/Onboarding/Suspended/Resigned/Terminated/Archived/Draft"),
        new("DOB", "DOB", false, "1990-06-15", "Format: yyyy-MM-dd"),
        new("IdentityUser", "IdentityUser", false, "user@company.com", "Link by email")
    ];
}
```

- [ ] **5.7 — EmployeeImportErrorFileGenerator.cs**

```csharp
using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.BulkImport.Services;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportErrorFileGenerator(
    BulkExcelTemplateService templateService,
    EmployeeImportTemplateProvider templateProvider)
    : IBulkImportErrorFileGenerator
{
    public byte[] GenerateErrorFile(
        IReadOnlyCollection<ImportPreviewRow> previewRows,
        IReadOnlyCollection<BulkImportRowResult> rowResults,
        IReadOnlyCollection<ImportColumnDefinition> columns)
    {
        return templateService.GenerateErrorFile(previewRows, rowResults, columns);
    }
}
```

- [ ] **5.8 — Verify compile**: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/`

---

## Task 6: CQRS Commands (Upload, Preview, Execute)

**Files:**
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeImport/UploadImportFileCommand.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeImport/UploadImportFileHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeImport/PreviewImportCommand.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeImport/PreviewImportHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeImport/ExecuteImportCommand.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeImport/ExecuteImportHandler.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeImport/EmployeeImportValidators.cs`
- Create: `Anemoi.Hr/Anemoi.Hr.Application/Cqrs/Commands/EmployeeImport/EmployeeImportResponses.cs`

- [ ] **6.1 — UploadImportFileCommand.cs**

```csharp
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed record UploadImportFileCommand(
    string FileName,
    long FileLength,
    byte[] FileBytes) : ICommandResult<EmployeeImportPreviewResponse>;
```

- [ ] **6.2 — UploadImportFileHandler.cs** (single parse + validate → store preview JSON)

```csharp
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.BulkImport.Services;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.BulkImport.EmployeeImport;
using Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.BulkImport;
using OneOf;
using System.Text.Json;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed class UploadImportFileHandler(
    BulkExcelParserService parser,
    SyntaxValidator syntaxValidator,
    BusinessValidator businessValidator,
    DuplicateValidator duplicateValidator,
    ReferenceValidator referenceValidator,
    ISqlRepository<BulkImportJob> jobRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UploadImportFileCommand, OneOf<EmployeeImportPreviewResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeImportPreviewResponse, ErrorDetailResponse>> Handle(
        UploadImportFileCommand request, CancellationToken ct)
    {
        var ext = Path.GetExtension(request.FileName)?.ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".csv")
            return HrErrorResponses.Create(HrBusinessErrorCodes.InvalidFileFormat);

        if (request.FileLength > BulkImportConstants.MaxFileSizeBytes)
            return HrErrorResponses.Create(HrBusinessErrorCodes.FileTooLarge);

        // Parse
        IReadOnlyCollection<Dictionary<string, string>> parsedData;
        try { parsedData = parser.Parse(request.FileBytes, request.FileName); }
        catch (Exception ex)
            { return HrErrorResponses.Create(HrBusinessErrorCodes.FileParseFailed, ex.Message); }

        if (parsedData.Count == 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportFileEmpty);
        if (parsedData.Count > BulkImportConstants.MaxUploadRows)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportTooManyRows,
                $"Max {BulkImportConstants.MaxUploadRows} rows, got {parsedData.Count}");

        // Convert to row DTOs
        var rows = parsedData.Select(d => new EmployeeImportRowDto { RawData = d }).ToList();

        // Stage 1: Syntax validation
        var allErrors = new List<BulkImportValidationError>();
        for (var i = 0; i < rows.Count; i++)
            allErrors.AddRange(await syntaxValidator.ValidateAsync(rows[i], i, ct));

        // Stage 2: Business validation
        for (var i = 0; i < rows.Count; i++)
            allErrors.AddRange(await businessValidator.ValidateAsync(rows[i], i, ct));

        // Stage 3: Duplicate validation (in-file)
        allErrors.AddRange(duplicateValidator.ValidateInFile(rows));

        // Stage 3: Duplicate validation (against DB)
        allErrors.AddRange(await duplicateValidator.ValidateAgainstDatabaseAsync(rows, ct));

        // Build row results
        var rowResults = new List<BulkImportRowResult>(rows.Count);
        var validRows = new List<EmployeeImportRowDto>();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowErrors = allErrors.Where(e => e.RowIndex == i).ToList();
            var hasError = rowErrors.Any(e => e.Severity == "Error");
            rowResults.Add(new BulkImportRowResult(i,
                hasError ? BulkImportRowStatus.Failed : BulkImportRowStatus.Success,
                null, rowErrors.Count > 0 ? rowErrors : null));
            if (!hasError) validRows.Add(rows[i]);
        }

        // Stage 4: Reference validation — but only during execute, not here
        // (Reference data may change between upload and execute)

        // Build preview model for storage
        var previewRows = rows.Select((r, i) =>
        {
            var rErrors = allErrors.Where(e => e.RowIndex == i).ToList();
            var hasErr = rErrors.Any(e => e.Severity == "Error");
            var hasWarn = rErrors.Any(e => e.Severity == "Warning");
            return new ImportPreviewRow(
                i, r.RawData,
                hasErr ? "Error" : hasWarn ? "Warning" : "Valid",
                rErrors.Count > 0 ? rErrors.Select(e =>
                    new BulkImportJobError(e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList() : null);
        }).ToList();

        var preview = new ImportPreviewData(
            request.FileName,
            rows.Count,
            validRows.Count,
            allErrors.Count(e => e.Severity == "Warning"),
            allErrors.Count(e => e.Severity == "Error"),
            previewRows,
            allErrors.Select(e => new BulkImportJobError(
                e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList());

        // Store as audit record (Pending status)
        var jobId = new BulkImportJobId(IdGenerator.NextGuid());
        var job = new BulkImportJob
        {
            Id = jobId,
            EntityType = "Employee",
            OriginalFileName = request.FileName,
            StatusCode = BulkImportJobStatus.Pending,
            TotalRows = rows.Count,
            ImportedRows = 0,
            FailedRows = 0,
            PreviewDataJson = JsonSerializer.Serialize(preview),
            CreatedAt = DateTime.UtcNow
        };
        await jobRepository.CreateOneAsync(job, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new EmployeeImportPreviewResponse(
            jobId,
            preview.TotalRows,
            preview.ValidRows,
            preview.WarningCount,
            preview.ErrorCount,
            preview.AllErrors
                .Select(e => new BulkImportValidationError(
                    e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity))
                .ToList(),
            rowResults);
    }
}
```

- [ ] **6.3 — PreviewImportCommand.cs + Handler.cs**

```csharp
// PreviewImportCommand.cs
public sealed record PreviewImportCommand(BulkImportJobId JobId)
    : ICommandResult<EmployeeImportPreviewResponse>;

// PreviewImportHandler.cs
public sealed class PreviewImportHandler(
    ISqlRepository<BulkImportJob> jobRepository)
    : ICommandHandler<PreviewImportCommand, OneOf<EmployeeImportPreviewResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeImportPreviewResponse, ErrorDetailResponse>> Handle(
        PreviewImportCommand request, CancellationToken ct)
    {
        var job = await jobRepository.GetFirstByConditionAsync(
            j => j.Id == request.JobId, null, ct);
        if (job is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportJobNotFound);

        var preview = JsonSerializer.Deserialize<ImportPreviewData>(job.PreviewDataJson);
        if (preview is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportFileEmpty);

        return new EmployeeImportPreviewResponse(
            job.Id,
            preview.TotalRows,
            preview.ValidRows,
            preview.WarningCount,
            preview.ErrorCount,
            preview.AllErrors.Select(e => new BulkImportValidationError(
                e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList(),
            preview.Rows.Select(r =>
            {
                var errors = r.Errors?.Select(e => new BulkImportValidationError(
                    e.RowIndex, e.Column, e.Value, e.ErrorMessage, e.Severity)).ToList();
                return new BulkImportRowResult(r.RowIndex,
                    r.ValidationStatus == "Error" ? BulkImportRowStatus.Failed : BulkImportRowStatus.Success,
                    null, errors);
            }).ToList());
    }
}
```

- [ ] **6.4 — ExecuteImportCommand.cs + Handler.cs** (thin — delegates to orchestrator)

```csharp
// ExecuteImportCommand.cs
public sealed record ExecuteImportCommand(
    BulkImportJobId JobId,
    string? ActorUserId = null,
    string? ActorName = null) : ICommandResult<EmployeeImportResultResponse>;

// ExecuteImportHandler.cs
public sealed class ExecuteImportHandler(
    ISqlRepository<BulkImportJob> jobRepository,
    IImportOrchestrator orchestrator)
    : ICommandHandler<ExecuteImportCommand, OneOf<EmployeeImportResultResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeImportResultResponse, ErrorDetailResponse>> Handle(
        ExecuteImportCommand request, CancellationToken ct)
    {
        var job = await jobRepository.GetFirstByConditionAsync(
            j => j.Id == request.JobId, null, ct);
        if (job is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportJobNotFound);

        return await orchestrator.ExecuteImportAsync(
            job, request.ActorUserId ?? "", request.ActorName, ct);
    }
}
```

- [ ] **6.5 — EmployeeImportValidators.cs**

```csharp
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed class UploadImportFileValidator : AbstractValidator<UploadImportFileCommand>
{
    public UploadImportFileValidator()
    {
        RuleFor(x => x.FileBytes).NotEmpty().WithMessage("File content is required");
        RuleFor(x => x.FileLength)
            .LessThanOrEqualTo(BulkImportConstants.MaxFileSizeBytes)
            .WithMessage($"File too large (max {BulkImportConstants.MaxFileSizeBytes / 1024 / 1024}MB)");
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("FileName is required")
            .Must(n => n.EndsWith(".xlsx") || n.EndsWith(".csv"))
            .WithMessage("Only .xlsx and .csv files are supported");
    }
}

public sealed class ExecuteImportValidator : AbstractValidator<ExecuteImportCommand>
{
    public ExecuteImportValidator()
    {
        RuleFor(x => x.JobId).NotNull().WithMessage("JobId is required");
    }
}
```

- [ ] **6.6 — EmployeeImportResponses.cs** (all response DTOs in one file)

```csharp
using Anemoi.BuildingBlock.Application.BulkImport.Models;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed record EmployeeImportPreviewResponse(
    BulkImportJobId JobId,
    int TotalRows,
    int ValidRows,
    int WarningCount,
    int ErrorCount,
    IReadOnlyCollection<BulkImportValidationError> Errors,
    IReadOnlyCollection<BulkImportRowResult> RowResults);

public sealed record EmployeeImportResultResponse(
    BulkImportJobId JobId,
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    string Status,
    byte[]? ErrorFileBytes,
    string? ErrorFileName);
```

- [ ] **6.7 — Verify compile**: `dotnet build Anemoi.Hr/Anemoi.Hr.Application/`

---

## Task 7: CQRS Queries (History, Detail, Template, Error File)

**Files:**
- Same as plan v1 but adapted to new models (no raw file bytes stored, generate error from preview)

- [ ] **7.1 — GetImportHistoryQuery.cs + Handler** — same as v1 (reads BulkImportJob table)

- [ ] **7.2 — GetImportJobDetailQuery.cs + Handler** — same as v1 but pulls error details from stored JSON

- [ ] **7.3 — GetImportTemplateQuery.cs + Handler** — unchanged (generates template from columns)

- [ ] **7.4 — DownloadErrorFileQuery.cs + Handler** — regenerates error file from stored preview data + re-validates

```csharp
// DownloadErrorFileHandler.cs
public sealed class DownloadErrorFileHandler(
    ISqlRepository<BulkImportJob> jobRepository,
    SyntaxValidator syntaxValidator,
    BusinessValidator businessValidator,
    BulkExcelTemplateService templateService,
    EmployeeImportTemplateProvider templateProvider)
    : IQueryHandler<DownloadErrorFileQuery, OneOf<FileResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<FileResponse, ErrorDetailResponse>> Handle(
        DownloadErrorFileQuery request, CancellationToken ct)
    {
        var job = await jobRepository.GetFirstByConditionAsync(
            j => j.Id == request.JobId, null, ct);
        if (job is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportJobNotFound);

        var preview = JsonSerializer.Deserialize<ImportPreviewData>(job.PreviewDataJson);
        if (preview is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportFileEmpty);

        // Re-validate to get current error state
        var rows = preview.Rows.Select(r => new EmployeeImportRowDto { RawData = r.Data }).ToList();
        var allErrors = new List<BulkImportValidationError>();
        for (var i = 0; i < rows.Count; i++)
        {
            allErrors.AddRange(await syntaxValidator.ValidateAsync(rows[i], i, ct));
            allErrors.AddRange(await businessValidator.ValidateAsync(rows[i], i, ct));
        }

        var rowResults = preview.Rows.Select((r, i) =>
        {
            var errs = allErrors.Where(e => e.RowIndex == i).ToList();
            return new BulkImportRowResult(i,
                errs.Any(e => e.Severity == "Error") ? BulkImportRowStatus.Failed : BulkImportRowStatus.Success,
                null, errs.Count > 0 ? errs : null);
        }).ToList();

        // Use template service to generate error xlsx from preview data
        var errorBytes = templateService.GenerateErrorFile(
            preview.Rows, rowResults, templateProvider.Columns);

        var errorFileName = $"errors_{Path.GetFileNameWithoutExtension(job.OriginalFileName)}.xlsx";
        return new FileResponse(errorBytes, BulkImportConstants.ExcelContentType, errorFileName);
    }
}
```

- [ ] **7.5 — FileResponse.cs**

```csharp
namespace Anemoi.Hr.Application.Responses;

public sealed record FileResponse(byte[] FileBytes, string ContentType, string FileName);
```

- [ ] **7.6 — History response DTOs**

```csharp
namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;

public sealed record EmployeeImportHistoryResponse(
    BulkImportJobId Id,
    string OriginalFileName,
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    string Status,
    string ImportedBy,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    long ExecutionDurationMs);

public sealed record EmployeeImportJobDetailResponse(
    BulkImportJobId Id,
    string EntityType,
    string OriginalFileName,
    int TotalRows,
    int ImportedRows,
    int FailedRows,
    string Status,
    string ImportedBy,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    long ExecutionDurationMs,
    string? ErrorDetails);
```

---

## Task 8: Permissions + Error Codes + Localization

- Same as v1 plan — requires adding 4 new permissions to `Permissions.cs`, `HrPermissions.cs`, `SharedResource.*.resx`, and error codes to `HrBusinessErrorCodes.cs`.

---

## Task 9: API Controller

- Same structure as v1: `EmployeeImportController.cs` with endpoints for upload, preview, execute, template, history, detail, error-file.

---

## Task 10: ADR-035

**File:** `docs/architecture/ARCHITECTURE_DECISIONS.md` — append ADR-035.

```
# ADR-035 — Bulk Import Architecture

## Status
Approved

## Context
Phase 35 introduces a generic Bulk Import Framework. Multiple business modules
(Employee, Department, Position, Salary Grade, Leave Balance, etc.) need import
capability with consistent UX, validation, and audit.

## Decision

1. **Generic Pipeline + Strategy Pattern**: Interfaces in BuildingBlocks,
   entity-specific implementations in modules (Anemoi.Hr first).

2. **Synchronous Only (Phase 35)**: No background jobs, SignalR, or async queues.
   Interface design allows future async extension.

3. **Upload → Parse → Validate (4 stages) → Preview → Execute**:
   - Stage 1: Syntax validation (format, required, types)
   - Stage 2: Business validation (domain enums, status transitions)
   - Stage 3: Duplicate validation (in-file + in-database)
   - Stage 4: Reference validation (FKs: department, position, manager)

4. **No raw file bytes in database**: Parsed preview stored as JSON.
   Error files generated on demand from preview data + re-validation.

5. **Single SaveChanges ownership**: Import orchestrator owns SaveChanges.
   Entity handlers add to change tracker only.

6. **All-or-nothing transaction**: Import all rows or rollback entirely.
   No partial success.

7. **BulkImportJob is an audit record**: Not a rich aggregate.
   Properties for tracking, JSON for preview data.

8. **Permissions**: 4 new permissions (import, history, download, template).

## Implications
- Storage: jsonb columns replace bytea for uploaded files
- Error file generation requires re-validation (acceptable trade-off)
- Future modules only need: row DTO, validator, handler, template provider
```
