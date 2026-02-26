using System;
using System.Collections.Generic;
using System.Linq;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Workspace.Commands.MemberCommands.CreateMember;
using Anemoi.Contract.Workspace.Commands.MemberInvitationCommands.CreateMemberInvitation;
using Anemoi.Contract.Workspace.Commands.OrganizationCommands.CreateOrganization;
using Anemoi.Contract.Workspace.Commands.OrganizationCommands.UpdateOrganization;
using Anemoi.Contract.Workspace.Commands.WorkspaceCommands.CreateWorkspace;
using Anemoi.Contract.Workspace.Commands.WorkspaceCommands.UpdateWorkspace;
using Anemoi.Contract.Workspace.ModelIds;
using Anemoi.Contract.Workspace.Responses;
using Anemoi.Workspace.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Workspace.Application.Mappings;

[Mapper]
public partial class WorkspaceMapper(IUserIdGetter userIdGetter, IWorkspaceIdGetter workspaceIdGetter)
{
    // Workspace Mappings
    public Anemoi.Workspace.Domain.Models.Workspace ToWorkspace(CreateWorkspaceCommand command)
    {
        var workspace = MapToWorkspace(command);
        workspace.Id = new WorkspaceId(IdGenerator.NextGuid());
        workspace.SearchHint = command.Name.GenerateSearchHint();
        workspace.UserId = userIdGetter.UserId;
        workspace.CreatedTime = DateTime.UtcNow;

        var member = new Member
        {
            Id = new MemberId(IdGenerator.NextGuid()),
            UserId = workspace.UserId,
            WorkspaceId = workspace.Id,
            IsActivated = true,
            CreatedTime = DateTime.UtcNow
        };

        var organizationId = new OrganizationId(IdGenerator.NextGuid());
        workspace.Organizations =
        [
            new Organization
            {
                Id = organizationId,
                WorkspaceId = workspace.Id,
                Name = command.Name,
                SearchHint = command.Name.GenerateSearchHint(),
                SubDomain = command.SubDomain,
                CreatedTime = DateTime.UtcNow,
                MemberMapOrganizations =
                [
                    new MemberMapOrganization
                    {
                        Id = new MemberMapOrganizationId(IdGenerator.NextGuid()),
                        OrganizationId = organizationId,
                        MemberId = member.Id
                    }
                ]
            }
        ];
        workspace.Members = [member];
        return workspace;
    }

    [MapperIgnoreTarget(nameof(Anemoi.Workspace.Domain.Models.Workspace.Id))]
    [MapperIgnoreTarget(nameof(Anemoi.Workspace.Domain.Models.Workspace.SearchHint))]
    [MapperIgnoreTarget(nameof(Anemoi.Workspace.Domain.Models.Workspace.UserId))]
    [MapperIgnoreTarget(nameof(Anemoi.Workspace.Domain.Models.Workspace.CreatedTime))]
    [MapperIgnoreTarget(nameof(Anemoi.Workspace.Domain.Models.Workspace.Organizations))]
    [MapperIgnoreTarget(nameof(Anemoi.Workspace.Domain.Models.Workspace.Members))]
    private partial Anemoi.Workspace.Domain.Models.Workspace MapToWorkspace(CreateWorkspaceCommand command);

    public void UpdateWorkspace(UpdateWorkspaceCommand command, Anemoi.Workspace.Domain.Models.Workspace workspace)
    {
        MapToWorkspace(command, workspace);
        if (command.Name is { })
        {
            workspace.SearchHint = command.Name.GenerateSearchHint();
        }
    }

    [MapperIgnoreTarget(nameof(Anemoi.Workspace.Domain.Models.Workspace.Id))]
    [MapperIgnoreTarget(nameof(Anemoi.Workspace.Domain.Models.Workspace.SearchHint))]
    private partial void MapToWorkspace(UpdateWorkspaceCommand command, Anemoi.Workspace.Domain.Models.Workspace workspace);

    public partial WorkspaceResponse ToWorkspaceResponse(Anemoi.Workspace.Domain.Models.Workspace workspace);
    public partial IQueryable<WorkspaceResponse> ProjectToWorkspaceResponse(IQueryable<Anemoi.Workspace.Domain.Models.Workspace> query);

    // Organization Mappings
    public Organization ToOrganization(CreateOrganizationCommand command)
    {
        var organization = MapToOrganization(command);
        organization.Id = new OrganizationId(IdGenerator.NextGuid());
        organization.WorkspaceId = new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId));
        organization.CreatedTime = DateTime.UtcNow;
        return organization;
    }

    [MapperIgnoreTarget(nameof(Organization.Id))]
    [MapperIgnoreTarget(nameof(Organization.WorkspaceId))]
    [MapperIgnoreTarget(nameof(Organization.CreatedTime))]
    private partial Organization MapToOrganization(CreateOrganizationCommand command);

    public partial void UpdateOrganization(UpdateOrganizationCommand command, Organization organization);

    public OrganizationResponse ToOrganizationResponse(Organization organization)
    {
        var response = MapToOrganizationResponse(organization);
        response.ParentOrganizationId = organization.ParentOrganizationId?.ToString();
        response.MemberQuantity = organization.MemberMapOrganizations?.Count ?? 0;
        return response;
    }

    [MapperIgnoreTarget(nameof(OrganizationResponse.ParentOrganizationId))]
    [MapperIgnoreTarget(nameof(OrganizationResponse.MemberQuantity))]
    private partial OrganizationResponse MapToOrganizationResponse(Organization organization);

    public partial IQueryable<OrganizationResponse> ProjectToOrganizationResponse(IQueryable<Organization> query);

    // Member Mappings
    public Member ToMember(CreateMemberCommand command)
    {
        var member = MapToMember(command);
        member.Id = new MemberId(IdGenerator.NextGuid());
        member.CreatedTime = DateTime.UtcNow;
        member.IsActivated = true;
        if (command.RoleGroupIds is { Count: > 0 } roleGroupIds)
        {
            member.MemberMapRoleGroups = roleGroupIds.Select((a, index) => new MemberMapRoleGroup
            {
                Id = new MemberMapRoleGroupId(IdGenerator.NextGuid()),
                RoleGroupId = a,
                Order = index
            }).ToList();
        }
        return member;
    }

    [MapperIgnoreTarget(nameof(Member.Id))]
    [MapperIgnoreTarget(nameof(Member.CreatedTime))]
    [MapperIgnoreTarget(nameof(Member.IsActivated))]
    [MapperIgnoreTarget(nameof(Member.MemberMapRoleGroups))]
    private partial Member MapToMember(CreateMemberCommand command);

    public Member CloneMember(Member member)
    {
        var clone = MapMemberToMember(member);
        clone.Id = new MemberId(IdGenerator.NextGuid());
        return clone;
    }

    [MapperIgnoreTarget(nameof(Member.Id))]
    private partial Member MapMemberToMember(Member member);

    public partial MemberResponse ToMemberResponse(Member member);
    public partial MemberIdResponse ToMemberIdResponse(Member member);
    public partial IQueryable<MemberResponse> ProjectToMemberResponse(IQueryable<Member> query);

    // MemberInvitation Mappings
    public MemberInvitation ToMemberInvitation(MemberInvitationData data)
    {
        var invitation = MapToMemberInvitation(data);
        invitation.Id = new MemberInvitationId(IdGenerator.NextGuid());
        invitation.CreatedTime = DateTime.UtcNow;
        invitation.CreatorId = userIdGetter.UserId;
        invitation.WorkspaceId = new WorkspaceId(Guid.Parse(workspaceIdGetter.WorkspaceId));
        if (data.OrganizationIds is { Count: > 0 } organizationIds)
        {
            invitation.MemberInvitationMapOrganizations = organizationIds.Select(a => new MemberInvitationMapOrganization
            {
                Id = new MemberInvitationMapOrganizationId(IdGenerator.NextGuid()),
                MemberInvitationId = invitation.Id,
                OrganizationId = a
            }).ToList();
        }
        return invitation;
    }

    [MapperIgnoreTarget(nameof(MemberInvitation.Id))]
    [MapperIgnoreTarget(nameof(MemberInvitation.CreatedTime))]
    [MapperIgnoreTarget(nameof(MemberInvitation.CreatorId))]
    [MapperIgnoreTarget(nameof(MemberInvitation.WorkspaceId))]
    [MapperIgnoreTarget(nameof(MemberInvitation.MemberInvitationMapOrganizations))]
    private partial MemberInvitation MapToMemberInvitation(MemberInvitationData data);

    public partial MemberInvitationResponse ToMemberInvitationResponse(MemberInvitation invitation);
    public partial IQueryable<MemberInvitationResponse> ProjectToMemberInvitationResponse(IQueryable<MemberInvitation> query);

    // MemberMapRoleGroup Mappings
    public partial MemberMapRoleGroupResponse ToMemberMapRoleGroupResponse(MemberMapRoleGroup memberMapRoleGroup);
    public partial IQueryable<MemberMapRoleGroupResponse> ProjectToMemberMapRoleGroupResponse(IQueryable<MemberMapRoleGroup> query);

    // ErrorDetail Mappings
    public partial ErrorDetailResponse ToErrorDetailResponse(ErrorDetail errorDetail);
    public partial ErrorDetail ToErrorDetail(ErrorDetailResponse errorDetailResponse);
}
