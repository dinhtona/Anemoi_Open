using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Onboarding;

public sealed class OnboardingPlanTemplate : ValueObject
{
    private readonly List<OnboardingTaskTemplate> _taskTemplates = [];

    public OnboardingPlanTemplateId Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Status { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; }
    public IReadOnlyList<OnboardingTaskTemplate> TaskTemplates => _taskTemplates.AsReadOnly();

    private OnboardingPlanTemplate() { }

    private OnboardingPlanTemplate(
        OnboardingPlanTemplateId id,
        string name,
        string? description,
        string createdBy)
    {
        Id = id;
        Name = name;
        Description = description;
        Status = OnboardingPlanTemplateStatusCode.Active;
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        UpdatedAt = CreatedAt;
        UpdatedBy = createdBy;
    }

    public static OnboardingPlanTemplate Create(
        OnboardingPlanTemplateId id,
        string name,
        string? description,
        string createdBy)
    {
        return new OnboardingPlanTemplate(id, name, description, createdBy);
    }

    public void AddTaskTemplate(OnboardingTaskTemplate taskTemplate)
    {
        _taskTemplates.Add(taskTemplate);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string updatedBy,
        List<OnboardingTaskTemplate> updatedTasks)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        Version++;

        _taskTemplates.Clear();
        _taskTemplates.AddRange(updatedTasks);
    }

    public bool Activate()
    {
        if (Status == OnboardingPlanTemplateStatusCode.Active) return false;
        Status = OnboardingPlanTemplateStatusCode.Active;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool Deactivate()
    {
        if (Status == OnboardingPlanTemplateStatusCode.Inactive) return false;
        Status = OnboardingPlanTemplateStatusCode.Inactive;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public bool IsActive() => Status == OnboardingPlanTemplateStatusCode.Active;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
