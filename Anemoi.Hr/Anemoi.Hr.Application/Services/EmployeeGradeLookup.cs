using Anemoi.Hr.Application.Abstractions;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Services;

public sealed class EmployeeGradeLookup : IEmployeeGradeLookup
{
    private static readonly HashSet<string> ValidGrades = new(StringComparer.OrdinalIgnoreCase)
    {
        "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8", "G9", "G10"
    };

    public bool IsValidGrade(string gradeCode)
    {
        if (string.IsNullOrWhiteSpace(gradeCode)) return false;
        return ValidGrades.Contains(gradeCode.Trim());
    }
}
