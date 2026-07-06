using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Identity.Queries.RoleQueries.GetRoles;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;
using OneOf;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Queries.RoleQueries.GetRoles;

public sealed class UserRolesHandler(
    ISqlRepository<Role> sqlRepository,
    IdentityMapper mapper,
    ILogger logger)
    : EfQueryPaginationHandler<Role, GetUserRolesQuery, UserRoleResponse>(
        sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<Role, UserRoleResponse> BuildQueryFlow(
        IQueryListFilter<Role, UserRoleResponse> fromFlow,
        GetUserRolesQuery query) => fromFlow
        .WithFilter(null)
        .WithSpecialAction(x => x)
        .WithSortFieldWhenNotSet(r => r.Name)
        .WithSortedDirectionWhenNotSet(SortedDirection.Ascending);

    protected override Task<PaginationResponse<UserRoleResponse>> MapToResultAsync(GetUserRolesQuery query,
        OneOf<List<Role>, List<UserRoleResponse>> modelsOrResponses, long totalRecord)
        => Task.FromResult(new PaginationResponse<UserRoleResponse>(
            modelsOrResponses.Match(i => i.Select(mapper.ToUserRoleResponse).ToList(), rs => rs),
            totalRecord));
}