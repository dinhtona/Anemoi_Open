using Anemoi.Contract.Identity.Responses;
using FluentAssertions;
using Lambda.Identity.Application.SeedData;
using Xunit;

namespace Anemoi.Hr.Test;

public sealed class PermissionModelSeparationTests
{
    private const string HrEmployeeViewPermission = "hr.employee.view";

    [Fact]
    public void Employee_role_profile_contains_only_self_service_permissions()
    {
        EmployeeRoleSeeder.EmployeePermissions.Should().OnlyContain(permission =>
            permission.StartsWith("hr.ess.") ||
            permission.StartsWith("notification."));

        EmployeeRoleSeeder.EmployeePermissions.Should().NotContain(permission =>
            permission.StartsWith("hr.employee.") ||
            permission.StartsWith("hr.department.") ||
            permission.StartsWith("hr.position.") ||
            permission.StartsWith("hr.recruitment.") ||
            permission.StartsWith("hr.workflow.") ||
            permission.StartsWith("hr.payroll.") ||
            permission.Contains(".approve"));
    }

    [Fact]
    public void Hr_manager_position_with_employee_role_has_no_hr_permissions()
    {
        var employee = new EmployeePermissionScenario(
            PositionName: "HR Manager",
            RoleProfile: SystemRoleProfiles.Employee);

        employee.EffectivePermissions.Should().NotContain(HrEmployeeViewPermission);
    }

    [Fact]
    public void Developer_position_with_hr_system_role_has_hr_permissions()
    {
        var employee = new EmployeePermissionScenario(
            PositionName: "Developer",
            RoleProfile: SystemRoleProfiles.Hr);

        employee.EffectivePermissions.Should().Contain(HrEmployeeViewPermission);
    }

    [Fact]
    public void Hr_access_is_defined_by_explicit_system_role_profile_not_position_name()
    {
        SystemRoleProfiles.Employee.Permissions.Should().BeEquivalentTo(EmployeeRoleSeeder.EmployeePermissions);
        SystemRoleProfiles.Hr.Permissions.Should().Contain(HrEmployeeViewPermission);
        SystemRoleProfiles.Hr.Permissions.Should().Contain("hr.attendance.view");
        SystemRoleProfiles.Hr.Permissions.Should().NotContain("HR Manager");
        SystemRoleProfiles.Hr.Permissions.Should().NotContain("HrManager");
    }

    [Fact]
    public void Dev_test_users_receive_employee_role_group_only_by_default()
    {
        DevTestRoleAssignments.RoleGroupsByEmail.Values.Should()
            .OnlyContain(roleGroups =>
                roleGroups.Length == 1 && roleGroups[0] == SystemRoleProfiles.Employee.Name);

        DevTestRoleAssignments.RoleGroupsByEmail.Keys.Should().Contain([
            "linh.nguyen@anemoi.test",
            "minh.tran@anemoi.test",
            "an.pham@anemoi.test",
            "mai.le@anemoi.test",
            "khoa.do@anemoi.test"
        ]);
    }

    [Fact]
    public void User_response_separates_display_roles_from_permission_codes()
    {
        var response = new UserResponse
        {
            Roles = ["Employee", "HR"],
            Permissions = ["hr.ess.profile.view", "hr.employee.view"]
        };

        response.Roles.Should().NotContain(role => role.StartsWith("hr."));
        response.Permissions.Should().Contain("hr.employee.view");
    }

    [Fact]
    public void Frontend_permission_helper_reads_permissions_not_roles_for_permission_codes()
    {
        var sourcePath = FindRepoFile("cody-web-app/src/hooks/usePermissions.ts");
        var source = File.ReadAllText(sourcePath);

        source.Should().Contain("user.permissions?.includes(permission)");
        source.Should().Contain("permissions.some((p) => user.permissions?.includes(p))");
        source.Should().NotContain("user.roles.includes(permission)");
        source.Should().NotContain("user.roles?.includes(p)");
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

    private sealed record EmployeePermissionScenario(string PositionName, SystemRoleProfile RoleProfile)
    {
        public IReadOnlyCollection<string> EffectivePermissions => RoleProfile.Permissions;
    }
}
