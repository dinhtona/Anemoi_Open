namespace Anemoi.Hr.Application.Responses;

public sealed class AttendanceRecordDetailResponse : AttendanceRecordResponse
{
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }
    public string PeriodCode { get; set; }
}
