using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryOneFlow;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryOne;
using Anemoi.Contract.Identity.Errors;
using Anemoi.Contract.Identity.Queries.IdentityQueries.CheckUserExist;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Queries.IdentityQueries.CheckUserExist;

public sealed class CheckUserExistHandler(
    ISqlRepository<User> sqlRepository,
    IdentityMapper mapper,
    ILogger logger)
    : EfQueryOneHandler<User,
        CheckUserExistQuery, UserWithEmailResponse>(sqlRepository, logger)
{
    protected override IQueryOneFlowBuilder<User, UserWithEmailResponse> BuildQueryFlow(
        IQueryOneFilter<User, UserWithEmailResponse> fromFlow,
        CheckUserExistQuery query)
        => fromFlow
            .WithFilter(x => x.Email == query.Email && x.IsActivated)
            .WithSpecialAction(mapper.ProjectToUserWithEmailResponse)
            .WithErrorIfNull(IdentityErrorDetail.UserError.NotFound());
}