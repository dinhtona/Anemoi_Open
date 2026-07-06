using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Onboarding;

public sealed class OnboardingTaskTemplate : ValueObject
{
    public OnboardingTaskTemplateId Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string AssigneeType { get; private set; } // "Role" only in Phase 26
    public string? AssigneeRoleCode { get; private set; }
    public int OffsetDays { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsRequired { get; private set; }

    private OnboardingTaskTemplate() { }

    private OnboardingTaskTemplate(
        OnboardingTaskTemplateId id,
        string title,
        string? description,
        string assigneeType,
        string? assigneeRoleCode,
        int offsetDays,
        int sortOrder,
        bool isRequired)
    {
        Id = id;
        Title = title;
        Description = description;
        AssigneeType = assigneeType;
        AssigneeRoleCode = assigneeRoleCode;
        OffsetDays = offsetDays;
        SortOrder = sortOrder;
        IsRequired = isRequired;
    }

    public static OnboardingTaskTemplate Create(
        OnboardingTaskTemplateId id,
        string title,
        string? description,
        string? assigneeRoleCode,
        int offsetDays,
        int sortOrder,
        bool isRequired = true)
    {
        return new OnboardingTaskTemplate(
            id, title, description, "Role", assigneeRoleCode,
            offsetDays, sortOrder, isRequired);
    }

    public void UpdateDetails(
        string title,
        string? description,
        string? assigneeRoleCode,
        int offsetDays,
        int sortOrder,
        bool isRequired)
    {
        Title = title;
        Description = description;
        AssigneeRoleCode = assigneeRoleCode;
        OffsetDays = offsetDays;
        SortOrder = sortOrder;
        IsRequired = isRequired;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
