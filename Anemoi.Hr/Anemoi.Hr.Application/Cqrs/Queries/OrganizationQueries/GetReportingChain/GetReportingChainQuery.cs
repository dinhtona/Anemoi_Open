using System.Collections.Generic;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetReportingChain;

public sealed record GetReportingChainQuery(EmployeeId EmployeeId)
    : IQueryOne<List<ReportingRelationshipResponse>>;
