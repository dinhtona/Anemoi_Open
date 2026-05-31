using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Identity.Queries.UserQueries.GetUserWithEmailsByEmails;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Queries.UserQueries.GetUserWithEmailsByEmails;

public sealed class GetUserWithEmailsByEmailsHandler(
    ISqlRepository<User> sqlRepository,
    IdentityMapper mapper,
    ILogger logger)
    : EfQueryCollectionHandler<User, GetUserWithEmailsByEmailsQuery, UserResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<User, UserResponse> BuildQueryFlow(
        IQueryListFilter<User, UserResponse> fromFlow, GetUserWithEmailsByEmailsQuery query)
        => fromFlow
            .WithFilter(x => query.Emails.Contains(x.Email))
            .WithSpecialAction(mapper.ProjectToUserResponse)
            .WithSortFieldWhenNotSet(x => x.CreatedTime)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);
}