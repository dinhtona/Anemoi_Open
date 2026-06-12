namespace Anemoi.Hr.Application.Configurations;

public sealed class HrSettings
{
    public string BusinessTimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    public int MonthlyAccrualCheckIntervalHours { get; set; } = 12;
    public int MonthlyAccrualRunHour { get; set; } = 0;
    public int ContractExpirationAlertDays { get; set; } = 30;
    public int OvertimeMaxHoursPerRequest { get; set; } = 12;
    public int OvertimeHistoricalDaysLimit { get; set; } = 365;
}
