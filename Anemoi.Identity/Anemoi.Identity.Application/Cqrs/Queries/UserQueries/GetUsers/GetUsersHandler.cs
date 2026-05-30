using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries.QueryFlow.QueryManyFlow;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Infrastructure.RequestHandlers.Queries.EntityFramework.EfQueryMany;
using Anemoi.Contract.Identity.Queries.UserQueries.GetUsers;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.Abstractions;
using Anemoi.Identity.Application.Mappings;
using Anemoi.Identity.Domain.Models;
using Anemoi.Contract.Identity.ModelIds;
using OneOf;
using Serilog;

namespace Anemoi.Identity.Application.Cqrs.Queries.UserQueries.GetUsers;

public sealed class GetUsersHandler(
    ISqlRepository<User> sqlRepository,
    ISqlRepository<UserMapRoleGroup> userMapRoleGroupRepository,
    IUserRepository userRepository,
    IdentityMapper mapper,
    ILogger logger)
    : EfQueryPaginationHandler<User, GetUsersQuery, UserResponse>(sqlRepository, logger)
{
    protected override IQueryListFlowBuilder<User, UserResponse> BuildQueryFlow(
        IQueryListFilter<User, UserResponse> fromFlow, GetUsersQuery query) =>
        fromFlow
            .WithFilter(BuildFilter(query).And(x => x.IsActivated))
            .WithSpecialAction(a => a)
            .WithSortFieldWhenNotSet(x => x.CreatedTime)
            .WithSortedDirectionWhenNotSet(SortedDirection.Descending);

    private static Expression<Func<User, bool>> BuildFilter(GetUsersQuery query)
    {
        Expression<Func<User, bool>> searchFilterEmails = query.SearchKey switch
        {
            { } val => a => a.Email.Contains(val),
            _ => _ => true
        };

        var searchHint = query.SearchKey.GenerateSearchHint();
        Expression<Func<User, bool>> nameFilter = searchHint switch
        {
            { } val => a => a.SearchHint.Contains(val),
            _ => _ => true
        };

        Expression<Func<User, bool>> phoneNumber = query.SearchKey switch
        {
            { } val => a => a.PhoneNumber.Contains(val),
            _ => _ => true
        };

        return ExpressionHelper.CombineAnd(searchFilterEmails, nameFilter, phoneNumber);
    }

    protected override async Task<PaginationResponse<UserResponse>> MapToResultAsync(GetUsersQuery query,
        OneOf<List<User>, List<UserResponse>> modelsOrResponses, long totalRecord)
    {
        if (modelsOrResponses.IsT1) return new PaginationResponse<UserResponse>(modelsOrResponses.AsT1, totalRecord);
        var users = modelsOrResponses.AsT0;
        var responses = users.Select(mapper.ToUserResponse).ToList();

        foreach (var response in responses)
        {
            if (Guid.TryParse(response.UserId, out var userGuid))
            {
                var userId = new UserId(userGuid);
                var roleGroups = await userMapRoleGroupRepository.GetManyByConditionAsync(
                    x => x.UserId == userId,
                    token: default);
                response.RoleGroupIds = roleGroups.Select(rg => rg.RoleGroupId.ToString()).ToList();

                var user = users.FirstOrDefault(u => u.UserId == userId);
                if (user is not null)
                {
                    response.Roles = (await userRepository.GetEffectiveRolesAsync(user)).ToList();
                }
            }
        }

        return new PaginationResponse<UserResponse>(responses, totalRecord);
    }
}
