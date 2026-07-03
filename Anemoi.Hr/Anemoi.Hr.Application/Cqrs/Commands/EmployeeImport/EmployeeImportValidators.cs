using Anemoi.BuildingBlock.Application.BulkImport.Models;
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
