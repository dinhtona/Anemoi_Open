using System;

namespace Anemoi.Hr.Application.Events;

public sealed record OvertimeRequestCreatedIntegrationEvent(
    string OvertimeRequestId,
    string EmployeeId);

public sealed record OvertimeRequestApprovedIntegrationEvent(
    string OvertimeRequestId,
    string EmployeeId);

public sealed record OvertimeRequestRejectedIntegrationEvent(
    string OvertimeRequestId,
    string EmployeeId);

public sealed record OvertimeRequestCancelledIntegrationEvent(
    string OvertimeRequestId,
    string EmployeeId);
