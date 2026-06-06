namespace Anemoi.Hr.Application.Responses;

public sealed class ChangeSalaryResponse
{
    public string EmployeeSalaryId { get; set; }
    public bool ValidationBypassed { get; set; }
    public string WarningCode { get; set; }
}
