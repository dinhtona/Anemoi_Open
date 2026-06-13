namespace Anemoi.Hr.Application.Configurations;

public sealed class HrSettings
{
    public string BusinessTimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    public int MonthlyAccrualCheckIntervalHours { get; set; } = 12;
    public int MonthlyAccrualRunHour { get; set; } = 0;
    public int ContractExpirationAlertDays { get; set; } = 30;
    public int OvertimeMaxHoursPerRequest { get; set; } = 12;
    public int OvertimeHistoricalDaysLimit { get; set; } = 365;
    public int ShiftMaxHoursPerDay { get; set; } = 16;
    public string PayslipDocumentStorageRoot { get; set; } = "temp_payslip_documents";
    public string PayslipDocumentPublicBaseUrl { get; set; } = "http://localhost:5000/api/hr/payslips";
    public bool UseSmtp { get; set; } = false;
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 25;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpEnableSsl { get; set; } = false;
    public string SmtpSenderEmail { get; set; } = "no-reply@anemoi.com";
    public string SmtpSenderName { get; set; } = "Anemoi HR";
}
