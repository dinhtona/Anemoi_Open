using System.Collections.Generic;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetOrganizationTree;

public sealed record GetOrganizationTreeQuery : IQueryOne<List<OrganizationNodeResponse>>;
