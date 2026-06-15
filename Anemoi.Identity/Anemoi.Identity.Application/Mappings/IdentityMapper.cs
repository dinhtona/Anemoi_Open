// RMG012/RMG020: Intentional - wrapper methods set identity framework properties,
// navigation entities, and computed fields manually after auto-mapping
#pragma warning disable RMG012, RMG020

using System;
using System.Linq;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.Commands.RoleGroupCommands.CreateRoleGroup;
using Anemoi.Contract.Identity.Commands.RoleGroupCommands.UpdateRoleGroup;
using Anemoi.Contract.Identity.Commands.UserCommands.CreateUser;
using Anemoi.Contract.Identity.Commands.UserCommands.UpdateUser;
using Anemoi.Contract.Identity.Commands.IdentityCommands.UpdateUserRoleGroup;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Identity.Responses;
using Anemoi.Identity.Application.IdentityResults;
using Anemoi.Identity.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Riok.Mapperly.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;

namespace Anemoi.Identity.Application.Mappings;

[Mapper]
public partial class IdentityMapper(IPasswordHasher<User> passwordHasher, IUserIdGetter userIdGetter)
{
    // User Mappings
    public User ToUser(CreateUserCommand command)
    {
        var user = MapToUser(command);
        user.UserId = new UserId(IdGenerator.NextGuid());
        user.Id = user.UserId.Value;
        user.UserName = command.Email;
        user.NormalizedUserName = user.UserName?.ToUpper();
        user.LockoutEnabled = false;
        user.CreatedTime = DateTime.UtcNow;
        user.SecurityStamp = IdGenerator.NextGuid().ToString();
        user.SearchHint = $"{command.FirstName} {command.LastName}".Trim().GenerateSearchHint();
        
        if (!string.IsNullOrEmpty(command.Password))
        {
            user.PasswordHash = passwordHasher.HashPassword(user, command.Password);
            user.ChangedPasswordTime = DateTime.UtcNow;
        }

        return user;
    }

    [MapperIgnoreTarget(nameof(User.Id))]
    [MapperIgnoreTarget(nameof(User.UserId))]
    [MapperIgnoreTarget(nameof(User.UserName))]
    [MapperIgnoreTarget(nameof(User.NormalizedUserName))]
    [MapperIgnoreTarget(nameof(User.PasswordHash))]
    [MapperIgnoreTarget(nameof(User.ChangedPasswordTime))]
    [MapperIgnoreTarget(nameof(User.SearchHint))]
    [MapperIgnoreTarget(nameof(User.LockoutEnabled))]
    [MapperIgnoreTarget(nameof(User.CreatedTime))]
    [MapperIgnoreTarget(nameof(User.SecurityStamp))]
    private partial User MapToUser(CreateUserCommand command);

    public void UpdateUser(UpdateUserCommand command, User user)
    {
        MapToUser(command, user);
        if (command.FirstName is { } || command.LastName is { })
        {
             user.SearchHint = $"{command.FirstName ?? user.FirstName} {command.LastName ?? user.LastName}".Trim().GenerateSearchHint();
        }
    }

    [MapperIgnoreTarget(nameof(User.Id))]
    [MapperIgnoreTarget(nameof(User.UserId))]
    [MapperIgnoreTarget(nameof(User.Email))]
    [MapperIgnoreTarget(nameof(User.SearchHint))]
    private partial void MapToUser(UpdateUserCommand command, User user);

    [MapProperty(nameof(User.UserId), nameof(UserResponse.Id))]
    [MapProperty(nameof(User.UserId), nameof(UserResponse.UserId))]
    public partial UserResponse ToUserResponse(User user);
    public partial UserWithEmailResponse ToUserWithEmailResponse(User user);
    
    public partial IQueryable<UserResponse> ProjectToUserResponse(IQueryable<User> query);
    public partial IQueryable<UserWithEmailResponse> ProjectToUserWithEmailResponse(IQueryable<User> query);

    public AuthenticationSuccessResponse ToAuthenticationSuccessResponse(RefreshToken refreshToken)
    {
        return new AuthenticationSuccessResponse
        {
            RefreshToken = refreshToken.Id.ToString(),
            Token = refreshToken.UserToken,
            ExpiredIn = refreshToken.TokenExpiryTime
        };
    }

    // RefreshToken Mappings
    [MapProperty(nameof(IdentitySuccess.TokenExpiryTime), nameof(RefreshToken.TokenExpiryTime))]
    [MapProperty(nameof(IdentitySuccess.RefreshTokenExpiryTime), nameof(RefreshToken.ExpiryDate))]
    public partial RefreshToken ToRefreshToken(IdentitySuccess success);

    [MapProperty(nameof(IdentitySuccess.TokenExpiryTime), nameof(RefreshToken.TokenExpiryTime))]
    [MapProperty(nameof(IdentitySuccess.RefreshTokenExpiryTime), nameof(RefreshToken.ExpiryDate))]
    public partial void UpdateRefreshToken(IdentitySuccess success, RefreshToken refreshToken);

    // RoleGroup Mappings
    public RoleGroup ToRoleGroup(CreateRoleGroupCommand command)
    {
        var roleGroup = MapToRoleGroup(command);
        roleGroup.Id = new RoleGroupId(IdGenerator.NextGuid());
        roleGroup.CreatedTime = DateTime.UtcNow;
        roleGroup.CreatorId = new UserId(Guid.Parse(userIdGetter.UserId));
        roleGroup.SearchHint = command.Name.GenerateSearchHint();

        if (command.IdentityRoleIds is { Count: > 0 } identityRoleIds)
        {
            roleGroup.RoleGroupMapRoles = identityRoleIds.Select(roleId => new RoleGroupMapRole
            {
                Id = new RoleGroupMapUserRoleId(IdGenerator.NextGuid()),
                RoleGroupId = roleGroup.Id,
                RoleId = roleId
            }).ToList();
        }

        if (command.RoleGroupClaims is { Count: > 0 } claims)
        {
            roleGroup.RoleGroupClaims = claims.Select(claim => new RoleGroupClaim
            {
                Id = new RoleGroupClaimId(IdGenerator.NextGuid()),
                Key = claim.Key,
                Value = claim.Value
            }).ToList();
        }

        return roleGroup;
    }

    [MapperIgnoreTarget(nameof(RoleGroup.Id))]
    [MapperIgnoreTarget(nameof(RoleGroup.CreatedTime))]
    [MapperIgnoreTarget(nameof(RoleGroup.CreatorId))]
    [MapperIgnoreTarget(nameof(RoleGroup.SearchHint))]
    [MapperIgnoreTarget(nameof(RoleGroup.RoleGroupMapRoles))]
    [MapperIgnoreTarget(nameof(RoleGroup.RoleGroupClaims))]
    private partial RoleGroup MapToRoleGroup(CreateRoleGroupCommand command);

    public void UpdateRoleGroup(UpdateRoleGroupCommand command, RoleGroup roleGroup)
    {
        MapToRoleGroup(command, roleGroup);
        roleGroup.UpdatedTime = DateTime.UtcNow;
        roleGroup.UpdaterId = new UserId(Guid.Parse(userIdGetter.UserId));
        if (command.Name is { })
        {
            roleGroup.SearchHint = command.Name.GenerateSearchHint();
        }
    }

    [MapperIgnoreTarget(nameof(RoleGroup.Id))]
    [MapperIgnoreTarget(nameof(RoleGroup.RoleGroupMapRoles))]
    [MapperIgnoreTarget(nameof(RoleGroup.RoleGroupClaims))]
    [MapperIgnoreTarget(nameof(RoleGroup.UpdatedTime))]
    [MapperIgnoreTarget(nameof(RoleGroup.UpdaterId))]
    [MapperIgnoreTarget(nameof(RoleGroup.SearchHint))]
    private partial void MapToRoleGroup(UpdateRoleGroupCommand command, RoleGroup roleGroup);

    [MapProperty(nameof(RoleGroup.RoleGroupMapRoles), nameof(RoleGroupResponse.IdentityRoles))]
    public partial RoleGroupResponse ToRoleGroupResponse(RoleGroup roleGroup);
    
    public partial IQueryable<RoleGroupResponse> ProjectToRoleGroupResponse(IQueryable<RoleGroup> query);

    [UserMapping(Default = true)]
    public RoleGroupsResponse ToRoleGroupsResponse(RoleGroup roleGroup)
    {
        var response = MapToRoleGroupsResponse(roleGroup);
        response.CreatorId = roleGroup.CreatorId?.ToString();
        response.CreatorName = roleGroup.CreatorId == null ? null : roleGroup.Creator?.FirstName;
        response.CreatorEmail = roleGroup.CreatorId == null ? null : roleGroup.Creator?.Email;
        response.UpdaterId = roleGroup.UpdaterId?.ToString();
        response.UpdaterName = roleGroup.Updater == null ? null : roleGroup.Updater?.FirstName;
        response.UpdaterEmail = roleGroup.Updater == null ? null : roleGroup.Updater?.Email;
        return response;
    }

    [MapperIgnoreTarget(nameof(RoleGroupsResponse.CreatorId))]
    [MapperIgnoreTarget(nameof(RoleGroupsResponse.CreatorName))]
    [MapperIgnoreTarget(nameof(RoleGroupsResponse.CreatorEmail))]
    [MapperIgnoreTarget(nameof(RoleGroupsResponse.UpdaterId))]
    [MapperIgnoreTarget(nameof(RoleGroupsResponse.UpdaterName))]
    [MapperIgnoreTarget(nameof(RoleGroupsResponse.UpdaterEmail))]
    private partial RoleGroupsResponse MapToRoleGroupsResponse(RoleGroup roleGroup);
    
    public partial IQueryable<RoleGroupsResponse> ProjectToRoleGroupsResponse(IQueryable<RoleGroup> query);

    public partial UserRoleGroupResponse ToUserRoleGroupResponse(UserMapRoleGroup userMapRoleGroup);

    [MapProperty(nameof(RoleGroupMapRole.RoleId), nameof(UserRoleResponse.Id))]
    [MapProperty($"{nameof(RoleGroupMapRole.Role)}.{nameof(Role.Name)}", nameof(UserRoleResponse.Name))]
    public partial UserRoleResponse ToUserRoleResponse(RoleGroupMapRole roleGroupMapRole);

    // UserMapRoleGroup Mapping
    public UserMapRoleGroup ToUserMapRoleGroup(UpdateUserRoleGroupCommand command)
    {
        return new UserMapRoleGroup
        {
            Id = new UserMapRoleGroupId(IdGenerator.NextGuid()),
            UserId = command.UserId
        };
    }

    // Role Mapping
    [MapProperty(nameof(Role.RoleId), nameof(UserRoleResponse.Id))]
    public partial UserRoleResponse ToUserRoleResponse(Role role);

    // IdentityPolicy Mapping
    public IdentityPolicyResponse ToIdentityPolicyResponse(IdentityPolicy policy)
    {
        return new IdentityPolicyResponse
        {
            Id = policy.Id.ToString(),
            Key = policy.Key,
            UserRoles = policy.IdentityPolicyMapRoles
                .Select(r => ToUserRoleResponse(r.Role))
                .OrderBy(r => r.Name)
                .ToList()
        };
    }

    public partial IQueryable<IdentityPolicyResponse> ProjectToIdentityPolicyResponse(IQueryable<IdentityPolicy> query);

    // ErrorDetail Mappings
    public ErrorDetailResponse ToErrorDetailResponse(ErrorDetail errorDetail)
    {
        return new ErrorDetailResponse
        {
            Code = errorDetail.Code,
            Messages = errorDetail.Messages
        };
    }

    public ErrorDetail ToErrorDetail(ErrorDetailResponse errorDetailResponse)
    {
        return new ErrorDetail
        {
            Code = errorDetailResponse.Code,
            Messages = errorDetailResponse.Messages
        };
    }
}
