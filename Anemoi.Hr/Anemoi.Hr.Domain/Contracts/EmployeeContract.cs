using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Contracts;

public sealed class EmployeeContract : ValueObject
{
    public EmployeeContractId Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public string ContractNumber { get; set; }
    public string ContractTypeCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly SignedDate { get; set; }
    public string StatusCode { get; set; }
    public string Notes { get; set; }
    public string AttachmentFileId { get; set; }
    public Guid? PreviousContractId { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ContractTerminationDetail TerminationDetail { get; set; }
    public Employee Employee { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
