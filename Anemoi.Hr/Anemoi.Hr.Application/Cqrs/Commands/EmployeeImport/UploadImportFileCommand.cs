using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed record UploadImportFileCommand(
    string FileName,
    long FileLength,
    byte[] FileBytes) : ICommandResult<EmployeeImportPreviewResponse>;
