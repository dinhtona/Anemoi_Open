using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.Domain.Events;

public sealed record SalaryValidationBypassedEvent(
    Guid EmployeeId,
    string GradeCode,
    decimal RequestedSalary,
    string Currency,
    string BypassReason,
    string CreatedBy,
    DateTime CreatedAt) : DomainEvent;
