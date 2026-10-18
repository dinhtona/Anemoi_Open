using System.Collections.Generic;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Models;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetApproversPreview;

public sealed record GetApproversPreviewQuery(EmployeeId EmployeeId, string EntityType)
    : IQueryOne<List<WorkflowApproverCandidate>>;
