using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed class WorkflowDefinition : Entity<WorkflowDefinitionId>
{
    private readonly List<WorkflowDefinitionStep> _steps = [];

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string WorkflowTypeCode { get; private set; }
    public string TargetEntityType { get; private set; }
    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<WorkflowDefinitionStep> Steps => _steps.AsReadOnly();

    private WorkflowDefinition() { }

    private WorkflowDefinition(
        WorkflowDefinitionId id,
        string code,
        string name,
        string? description,
        string workflowTypeCode,
        string targetEntityType,
        int version,
        List<WorkflowDefinitionStep> steps)
    {
        Id = id;
        Code = code;
        Name = name;
        Description =description;
        WorkflowTypeCode = workflowTypeCode;
        TargetEntityType = targetEntityType;
        Version = version;
        IsActive = false;
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
        _steps = steps;
    }

    public static WorkflowDefinition Create(
        WorkflowDefinitionId id,
        string code,
        string name,
        string? description,
        string workflowTypeCode,
        string targetEntityType,
        int version,
        List<WorkflowDefinitionStep> steps)
    {
        return new WorkflowDefinition(id, code, name, description, workflowTypeCode, targetEntityType, version, steps);
    }

    public void Activate()
    {
        if (_steps.Count == 0)
            throw new InvalidOperationException("Cannot activate a workflow definition with no steps.");
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceSteps(List<WorkflowDefinitionStep> newSteps)
    {
        if (IsActive)
            throw new InvalidOperationException("Cannot replace steps on an active workflow definition.");
        _steps.Clear();
        _steps.AddRange(newSteps);
        UpdatedAt = DateTime.UtcNow;
    }
}
