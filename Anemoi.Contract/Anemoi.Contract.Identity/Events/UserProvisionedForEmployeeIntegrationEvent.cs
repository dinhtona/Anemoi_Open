using System;

namespace Anemoi.Contract.Identity.Events;

public sealed record UserProvisionedForEmployeeIntegrationEvent(
    Guid EmployeeId,
    Guid UserId,
    string Email,
    string EmployeeCode);
