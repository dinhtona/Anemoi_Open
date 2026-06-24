using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using Anemoi.BuildingBlock.Application.Authorization;
using Anemoi.BuildingBlock.Application.Resources;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test;

public sealed class PermissionLocalizationTests
{
    private static readonly ResourceManager ResourceManager =
        new(typeof(SharedResource).FullName!, typeof(SharedResource).Assembly);

    [Theory]
    [InlineData("vi-VN", Permissions.HrAllowanceTypeManage, "Quản lý loại phụ cấp.")]
    [InlineData("vi-VN", Permissions.HrAnalyticsView, "Xem phân tích nhân sự.")]
    public void Permission_metadata_returns_vietnamese_description(
        string cultureName,
        string permissionCode,
        string expectedDescription)
    {
        var description = GetPermissionDescription(cultureName, permissionCode);

        description.Should().Be(expectedDescription);
        description.Should().NotStartWith("PermissionDescription");
    }

    [Theory]
    [InlineData("en-US", Permissions.HrAllowanceTypeManage, "Manage allowance types.")]
    [InlineData("en-US", Permissions.HrAnalyticsView, "View HR analytics.")]
    public void Permission_metadata_returns_english_description(
        string cultureName,
        string permissionCode,
        string expectedDescription)
    {
        var description = GetPermissionDescription(cultureName, permissionCode);

        description.Should().Be(expectedDescription);
        description.Should().NotStartWith("PermissionDescription");
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("vi-VN")]
    public void Permission_catalog_description_and_group_keys_have_culture_resources(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var permissionKeys = ReadPermissionResourceKeys();
        var missingKeys = permissionKeys
            .Where(key => string.IsNullOrWhiteSpace(ResourceManager.GetString(key, culture)))
            .ToArray();

        missingKeys.Should().BeEmpty();
    }

    private static string GetPermissionDescription(string cultureName, string permissionCode)
    {
        var permission = Permissions.Find(permissionCode);
        permission.Should().NotBeNull();

        return ResourceManager.GetString(
            permission!.DescriptionKey,
            CultureInfo.GetCultureInfo(cultureName))!;
    }

    private static IReadOnlyCollection<string> ReadPermissionResourceKeys()
    {
        var permissionsSource = File.ReadAllText(FindRepoFile(
            "Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs"));

        return Regex
            .Matches(permissionsSource, "\"(Permission(?:Description|Group)[^\"]+)\"")
            .Select(match => match.Groups[1].Value)
            .Distinct()
            .ToArray();
    }

    private static string FindRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate {relativePath} from {AppContext.BaseDirectory}");
    }
}
