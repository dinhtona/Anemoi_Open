using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRequisitionById;

public sealed record GetRequisitionByIdQuery(JobRequisitionId Id)
    : IQueryOne<JobRequisitionResponse>;
