using System.Globalization;
using System.Text.Json;
using Anemoi.BuildingBlock.Application.Resources;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Employees;
using Microsoft.Extensions.Localization;

namespace Anemoi.Hr.Application.Mappings;

public sealed class EmployeeHistoryMapper(IStringLocalizer<SharedResource> localizer)
{
    public EmployeeHistoryDto ToDto(EmployeeHistory history)
    {
        if (history is null) return null;

        var metadata = ParseMetadata(history.MetadataJson);

        return new EmployeeHistoryDto
        {
            Id = history.Id.Value.ToString(),
            EmployeeId = history.EmployeeId.Value.ToString(),
            EntityType = history.EntityType,
            EntityId = history.EntityId,
            EventType = history.EventType,
            Title = LocalizeTitle(history, metadata),
            Description = LocalizeDescription(history, metadata),
            MetadataJson = history.MetadataJson,
            OccurredAt = history.OccurredAt,
            ActorUserId = history.ActorUserId?.ToString(),
            ActorEmployeeId = history.ActorEmployeeId?.Value.ToString(),
            CorrelationId = history.CorrelationId
        };
    }

    private string LocalizeTitle(EmployeeHistory history, IReadOnlyDictionary<string, string?> metadata)
    {
        var key = $"Timeline.{history.EventType}.Title";
        var localized = localizer[key];
        return localized.ResourceNotFound || localized.Value == key || string.IsNullOrWhiteSpace(localized.Value)
            ? history.Title
            : localized.Value;
    }

    private string LocalizeDescription(EmployeeHistory history, IReadOnlyDictionary<string, string?> metadata)
    {
        var key = $"Timeline.{history.EventType}.Description";
        var localized = localizer[key];
        if (localized.ResourceNotFound || localized.Value == key || string.IsNullOrWhiteSpace(localized.Value))
            return history.Description;

        return string.Format(
            CultureInfo.CurrentUICulture,
            localized.Value,
            Get(metadata, "fullName", history.Description),
            Get(metadata, "employeeCode", string.Empty),
            Get(metadata, "fromStatus", string.Empty),
            Get(metadata, "toStatus", string.Empty),
            Get(metadata, "separationTypeCode", string.Empty),
            Get(metadata, "assetTag", string.Empty),
            Get(metadata, "name", history.Description),
            Get(metadata, "documentType", string.Empty),
            Get(metadata, "displayName", history.Description),
            Get(metadata, "content", history.Description));
    }

    private static string Get(IReadOnlyDictionary<string, string?> metadata, string key, string fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value! : fallback;

    private static Dictionary<string, string?> ParseMetadata(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(metadataJson);
            return raw is null
                ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                : raw.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ValueKind == JsonValueKind.String ? kvp.Value.GetString() : kvp.Value.ToString(),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
