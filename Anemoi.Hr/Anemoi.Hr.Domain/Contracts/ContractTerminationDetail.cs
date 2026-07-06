using Anemoi.BuildingBlock.Domain;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Contracts;

public sealed class ContractTerminationDetail : ValueObject
{
    public DateOnly TerminatedDate { get; set; }
    public string ReasonCode { get; set; }
    public string Notes { get; set; }
    public string TerminationAttachmentId { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TerminatedDate;
        yield return ReasonCode;
        yield return Notes ?? "";
        yield return TerminationAttachmentId ?? "";
    }
}
