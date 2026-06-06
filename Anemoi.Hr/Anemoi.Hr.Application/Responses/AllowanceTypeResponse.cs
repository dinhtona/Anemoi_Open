using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class AllowanceTypeResponse
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsActive { get; set; }
}
