using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class PayslipBatchResultResponse
{
    public int Total { get; set; }
    public int Succeeded { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public IReadOnlyCollection<PayslipBatchItemResult> Items { get; set; } = [];
}

public sealed class PayslipBatchItemResult
{
    public Guid PayslipId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string Status { get; set; } = default!;
    public string? ErrorCode { get; set; }
}
