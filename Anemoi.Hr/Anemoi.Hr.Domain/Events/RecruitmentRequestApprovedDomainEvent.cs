using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.Domain.Events;

public sealed record RecruitmentRequestApprovedDomainEvent(
    Guid RecruitmentRequestId, string RequestNumber, string ApprovedBy) : DomainEvent;
