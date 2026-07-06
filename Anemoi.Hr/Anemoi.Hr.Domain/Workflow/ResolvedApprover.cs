using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed record ResolvedApprover(
    UserId UserId,
    EmployeeId EmployeeId,
    string FullName,
    string Email,
    string ResolutionSource);
