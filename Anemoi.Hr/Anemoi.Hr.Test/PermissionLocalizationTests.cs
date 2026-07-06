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
        description.Should().NotStartWith("PermissionGroup");
        description.Should().NotStartWith("PERMISSIONGROUP");
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
        description.Should().NotStartWith("PermissionGroup");
        description.Should().NotStartWith("PERMISSIONGROUP");
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

    [Theory]
    [InlineData("en-US")]
    [InlineData("vi-VN")]
    public void All_permission_descriptions_are_translated_and_not_raw_keys(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var untranslated = new List<string>();

        foreach (var definition in Permissions.Definitions)
        {
            var value = ResourceManager.GetString(definition.DescriptionKey, culture);
            if (string.IsNullOrWhiteSpace(value))
                untranslated.Add($"{definition.DescriptionKey} (missing)");
            else if (value == definition.DescriptionKey)
                untranslated.Add($"{definition.DescriptionKey} (value == key)");
            else if (IsRawKey(value))
                untranslated.Add($"{definition.DescriptionKey} = '{value}'");
        }

        untranslated.Should().BeEmpty(
            $"All permission descriptions must be translated and not raw keys. Found: {string.Join(", ", untranslated)}");
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("vi-VN")]
    public void All_permission_group_names_are_translated_and_not_raw_keys(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        var groupKeys = Permissions.Definitions
            .Select(d => d.GroupKey)
            .Distinct()
            .ToList();

        var untranslated = new List<string>();
        foreach (var groupKey in groupKeys)
        {
            var value = ResourceManager.GetString(groupKey, culture);
            if (string.IsNullOrWhiteSpace(value))
                untranslated.Add($"{groupKey} (missing)");
            else if (value == groupKey)
                untranslated.Add($"{groupKey} (value == key)");
            else if (IsRawKey(value))
                untranslated.Add($"{groupKey} = '{value}'");
        }

        untranslated.Should().BeEmpty(
            $"All permission group names must be translated and not raw keys. Found: {string.Join(", ", untranslated)}");
    }

    [Fact]
    public void Hr_recruitment_permissions_have_vietnamese_translations()
    {
        var culture = CultureInfo.GetCultureInfo("vi-VN");
        var hrRecruitmentKeys = Permissions.Definitions
            .Where(d => d.Key.StartsWith("hr.recruitment"))
            .ToList();

        hrRecruitmentKeys.Should().NotBeEmpty("HR Recruitment permissions must exist in the catalog");

        foreach (var def in hrRecruitmentKeys)
        {
            var groupValue = ResourceManager.GetString(def.GroupKey, culture);
            groupValue.Should().NotBeNullOrWhiteSpace(
                $"Group '{def.GroupKey}' must have VI translation for {def.Key}");

            var descValue = ResourceManager.GetString(def.DescriptionKey, culture);
            descValue.Should().NotBeNullOrWhiteSpace(
                $"Description '{def.DescriptionKey}' must have VI translation for {def.Key}");

            descValue.Should().NotStartWith("PermissionDescription",
                $"Description for {def.Key} is a raw key in VI");
            groupValue.Should().NotBe("PERMISSIONGROUPHRRECRUITMENT",
                $"Group for {def.Key} is a raw uppercased key in VI");
        }
    }

    [Fact]
    public void Hr_recruitment_group_is_translated_in_vietnamese()
    {
        var culture = CultureInfo.GetCultureInfo("vi-VN");
        var value = ResourceManager.GetString("PermissionGroupHrRecruitment", culture);
        value.Should().Be("Tuyển dụng");
    }

    [Fact]
    public void Hr_recruitment_group_is_translated_in_english()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var value = ResourceManager.GetString("PermissionGroupHrRecruitment", culture);
        value.Should().Be("HR Recruitment");
    }

    [Theory]
    [InlineData("vi-VN", Permissions.HrRecruitmentAnalytics, "Xem phân tích tuyển dụng.")]
    [InlineData("vi-VN", Permissions.HrRecruitmentHire, "Hoàn tất thao tác tuyển dụng ứng viên.")]
    [InlineData("vi-VN", Permissions.HrRecruitmentInterview, "Quản lý phỏng vấn tuyển dụng.")]
    [InlineData("vi-VN", Permissions.HrRecruitmentView, "Xem hồ sơ tuyển dụng.")]
    [InlineData("vi-VN", Permissions.HrRecruitmentManage, "Quản lý cấu hình và hồ sơ tuyển dụng.")]
    public void Specific_hr_recruitment_permissions_have_vietnamese_translations(
        string cultureName, string permissionCode, string expectedDescription)
    {
        var description = GetPermissionDescription(cultureName, permissionCode);
        description.Should().Be(expectedDescription);
        description.Should().NotStartWith("PermissionDescription");
    }

    [Theory]
    [InlineData("en-US", Permissions.HrRecruitmentAnalytics, "View recruitment analytics.")]
    [InlineData("en-US", Permissions.HrRecruitmentHire, "Complete candidate hiring actions.")]
    [InlineData("en-US", Permissions.HrRecruitmentInterview, "Manage recruitment interviews.")]
    [InlineData("en-US", Permissions.HrRecruitmentView, "View recruitment records.")]
    [InlineData("en-US", Permissions.HrRecruitmentManage, "Manage recruitment configuration and records.")]
    public void Specific_hr_recruitment_permissions_have_english_translations(
        string cultureName, string permissionCode, string expectedDescription)
    {
        var description = GetPermissionDescription(cultureName, permissionCode);
        description.Should().Be(expectedDescription);
        description.Should().NotStartWith("PermissionDescription");
    }

    [Fact]
    public void No_permission_value_contains_raw_resource_key_patterns()
    {
        var cultures = new[] { "en-US", "vi-VN" };
        var rawKeyPatterns = new[] { "PermissionDescription", "PermissionGroup", "PERMISSIONGROUP" };

        foreach (var cultureName in cultures)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            var allPermissionKeys = ReadPermissionResourceKeys();

            foreach (var key in allPermissionKeys)
            {
                var value = ResourceManager.GetString(key, culture);
                if (string.IsNullOrWhiteSpace(value)) continue;

                foreach (var pattern in rawKeyPatterns)
                {
                    value.Should().NotContain(pattern,
                        $"Resource '{key}' in '{cultureName}' contains raw key pattern '{pattern}': '{value}'");
                }
            }
        }
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

    private static bool IsRawKey(string value) =>
        value.StartsWith("PermissionDescription", StringComparison.Ordinal) ||
        value.StartsWith("PermissionGroup", StringComparison.Ordinal) ||
        value.StartsWith("PERMISSIONGROUP", StringComparison.Ordinal);

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
