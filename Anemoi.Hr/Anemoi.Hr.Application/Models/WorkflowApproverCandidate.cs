using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Models;

public sealed record WorkflowApproverCandidate(
    int StepOrder,
    string ApproverType,
    string? ApproverValue,
    EmployeeId? ResolvedEmployeeId,
    string? ResolvedName,
    string? ResolvedEmail);
