using System.Collections.Generic;

namespace Anemoi.Hr.Application.Configurations;

public static class AttendanceStatusCodes
{
    public const string Present = "Present";
    public const string Absent = "Absent";
    public const string Leave = "Leave";
    public const string Holiday = "Holiday";

    public static readonly IReadOnlyCollection<string> All =
    [
        Present,
        Absent,
        Leave,
        Holiday
    ];
}
