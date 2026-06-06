using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Payroll;

public sealed class PayrollItem : ValueObject
{
    public PayrollItemId Id { get; set; }
    public PayrollRunId PayrollRunId { get; set; }
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }

    // Navigation
    public PayrollRun PayrollRun { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
