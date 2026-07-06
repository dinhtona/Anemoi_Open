#nullable enable
using System.Collections.Generic;

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
