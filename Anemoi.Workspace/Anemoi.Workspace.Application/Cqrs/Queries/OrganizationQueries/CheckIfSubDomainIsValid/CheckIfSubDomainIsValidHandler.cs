using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.Workspace.Errors;
using Anemoi.Contract.Workspace.Queries.OrganizationQueries.CheckIfSubDomainIsValid;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Domain.Models;
using Anemoi.Workspace.Domain.Models;
using Serilog;

namespace Anemoi.Workspace.Application.Cqrs.Queries.OrganizationQueries.CheckIfSubDomainIsValid;

public sealed class CheckIfSubDomainIsValidHandler(
    ISqlRepository<Organization> sqlRepository,
    ILogger logger)
    : EfQueryOneHandler<Organization, CheckIfSubDomainIsValidQuery, OrganizationIdResponse>(sqlRepository,
        logger)
{
    protected override IQueryOneFlowBuilder<Organization, OrganizationIdResponse> BuildQueryFlow(
        IQueryOneFilter<Organization, OrganizationIdResponse> fromFlow, CheckIfSubDomainIsValidQuery query)
        => fromFlow
            .WithFilter(x => x.SubDomain == query.SubDomain)
            .WithSpecialAction(x => x.Select(a => new OrganizationIdResponse { Id = a.Id.ToString() }))
            .WithErrorIfNull(WorkspaceErrorDetail.OrganizationError.NotFound());
}