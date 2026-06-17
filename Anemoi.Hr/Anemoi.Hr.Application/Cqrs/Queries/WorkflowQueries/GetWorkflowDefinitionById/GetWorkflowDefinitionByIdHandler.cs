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

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitionById;

public sealed class GetWorkflowDefinitionByIdHandler(
    ISqlRepository<WorkflowDefinition> repository,
    WorkflowMapper mapper)
    : IQueryHandler<GetWorkflowDefinitionByIdQuery, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        GetWorkflowDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(Guid.Parse(request.Id));
        var definition = await repository.GetQueryable()
            .Include(x => x.Steps.OrderBy(s => s.Sequence))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        return mapper.ToResponse(definition);
    }
}
