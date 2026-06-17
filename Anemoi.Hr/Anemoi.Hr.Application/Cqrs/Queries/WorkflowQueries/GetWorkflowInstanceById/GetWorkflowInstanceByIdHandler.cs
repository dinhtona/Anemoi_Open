using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstanceById;

public sealed class GetWorkflowInstanceByIdHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    WorkflowMapper mapper)
    : IQueryHandler<GetWorkflowInstanceByIdQuery, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        GetWorkflowInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var id = new WorkflowInstanceId(Guid.Parse(request.Id));
        var instance = await instanceRepository.GetQueryable()
            .Include(x => x.Steps.OrderBy(s => s.Sequence))
            .Include(x => x.Histories.OrderByDescending(h => h.PerformedAt))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        var definition = await definitionRepository.GetQueryable()
            .FirstOrDefaultAsync(d => d.Id == instance.WorkflowDefinitionId, cancellationToken);

        return mapper.ToResponse(instance, definition?.Name);
    }
}
