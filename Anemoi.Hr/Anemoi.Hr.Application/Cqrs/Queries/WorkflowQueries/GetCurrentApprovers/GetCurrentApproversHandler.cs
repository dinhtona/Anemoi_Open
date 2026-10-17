using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetCurrentApprovers;

public sealed class GetCurrentApproversHandler(IWorkflowEngine workflowEngine)
    : IQueryHandler<GetCurrentApproversQuery, OneOf<List<CurrentApproverResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<List<CurrentApproverResponse>, ErrorDetailResponse>> Handle(
        GetCurrentApproversQuery request, CancellationToken cancellationToken)
    {
        var approvers = await workflowEngine.GetCurrentApproversAsync(
            new WorkflowInstanceId(Guid.Parse(request.Id)), cancellationToken);

        return approvers
            .Select(a => new CurrentApproverResponse { UserId = a.Value.ToString() })
            .ToList();
    }
}
