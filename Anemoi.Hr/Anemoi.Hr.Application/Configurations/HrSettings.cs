namespace Anemoi.Hr.Application.Configurations;

public sealed class HrSettings
{
    public string BusinessTimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    public int MonthlyAccrualCheckIntervalHours { get; set; } = 12;
    public int MonthlyAccrualRunHour { get; set; } = 0;
}
