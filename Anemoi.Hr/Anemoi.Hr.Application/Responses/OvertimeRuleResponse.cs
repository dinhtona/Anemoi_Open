namespace Anemoi.Hr.Application.Responses;

public sealed class OvertimeRuleResponse
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public decimal WeekdayMultiplier { get; set; }
    public decimal WeekendMultiplier { get; set; }
    public decimal HolidayMultiplier { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
