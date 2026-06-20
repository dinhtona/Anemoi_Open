namespace Anemoi.Hr.Domain.Departments;

public static class DepartmentTypeCode
{
    public const string Functional = "functional";

    public static readonly string[] All = [Functional];

    public static bool IsValid(string value) => All.Contains(value);
}
