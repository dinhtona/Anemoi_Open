namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.Common;

public sealed record WorkflowDefinitionStepInput(
    int Sequence,
    string ApproverType,
    string? ApproverValue,
    bool IsRequired);
