using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.Domain.Events;

public sealed record RecruitmentRequestRejectedDomainEvent(
    Guid RecruitmentRequestId, string RequestNumber, string RejectedBy, string Reason) : DomainEvent;
