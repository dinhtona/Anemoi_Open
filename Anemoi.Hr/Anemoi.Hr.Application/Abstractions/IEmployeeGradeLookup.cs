namespace Anemoi.Hr.Application.Abstractions;

public interface IEmployeeGradeLookup
{
    bool IsValidGrade(string gradeCode);
}
