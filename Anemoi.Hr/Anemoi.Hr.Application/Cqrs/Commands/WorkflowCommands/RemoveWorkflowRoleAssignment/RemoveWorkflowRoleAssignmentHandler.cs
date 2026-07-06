using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RemoveWorkflowRoleAssignment;

public sealed class RemoveWorkflowRoleAssignmentHandler(
    ISqlRepository<WorkflowRoleAssignment> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveWorkflowRoleAssignmentCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        RemoveWorkflowRoleAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await repository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (assignment is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowRoleAssignmentNotFound);

        var removeResult = await repository.RemoveOneAsync(assignment, cancellationToken);
        if (removeResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return new SuccessResponse();
    }
}
