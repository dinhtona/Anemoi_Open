using System.Globalization;
using System.Resources;
using Anemoi.BuildingBlock.Application.Resources;

namespace Anemoi.BuildingBlock.Application.Authorization;

/// <summary>
/// Localized display metadata for a role/permission.
/// </summary>
public sealed record PermissionRoleMetadata(string Group, string Description, bool HasMissingKey);

/// <summary>
/// Resolves permission group/description display metadata from <see cref="SharedResource"/>
/// resource files. Per AGENTS.md permission translation rule: this localizer NEVER
/// fabricates a display name from the permission code (no ToDisplayName-style
/// sanitizer/fallback). A missing resource key is surfaced as the raw key itself
/// (with <see cref="PermissionRoleMetadata.HasMissingKey"/> set) so gaps are visible
/// and fail localization tests instead of being silently hidden.
/// </summary>
public sealed class PermissionMetadataLocalizer
{
    public const string OtherGroupKey = "PermissionGroupOther";
    public const string UnmappedDescriptionKey = "PermissionDescriptionUnmapped";

    private static readonly ResourceManager Resources =
        new(typeof(SharedResource).FullName!, typeof(SharedResource).Assembly);

    public PermissionRoleMetadata Localize(string roleName, CultureInfo culture)
    {
        var permission = Permissions.Find(roleName);

        var group = ResolveKey(permission?.GroupKey ?? OtherGroupKey, culture, out var groupMissing);

        string description;
        bool descriptionMissing;
        if (permission is null)
        {
            var template = ResolveKey(UnmappedDescriptionKey, culture, out descriptionMissing);
            description = string.Format(culture, template, roleName);
        }
        else
        {
            description = ResolveKey(permission.DescriptionKey, culture, out descriptionMissing);
        }

        return new PermissionRoleMetadata(group, description, groupMissing || descriptionMissing);
    }

    /// <summary>
    /// Resolves a single resource key. Returns the raw key itself when the resource
    /// is missing — never a fabricated display name.
    /// </summary>
    public string ResolveKey(string key, CultureInfo culture, out bool isMissing)
    {
        var value = Resources.GetString(key, culture);
        isMissing = string.IsNullOrWhiteSpace(value);
        return isMissing ? key : value!;
    }
}
