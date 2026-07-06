using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.Domain.Events;

public sealed record RecruitmentRequestSubmittedDomainEvent(
    Guid RecruitmentRequestId, string RequestNumber, string RequestedBy) : DomainEvent;
