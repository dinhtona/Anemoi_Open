using System;
using Anemoi.Contract.Notification.Constants;
using FluentValidation;

namespace Anemoi.Contract.Notification.Events;

public sealed class DataChangeOccurredIntegrationEventValidator : AbstractValidator<DataChangeOccurredIntegrationEvent>
{
    public DataChangeOccurredIntegrationEventValidator()
    {
        RuleFor(x => x.Resource)
            .NotEmpty().WithMessage("Resource must not be empty.")
            .MaximumLength(100).WithMessage("Resource must not exceed 100 characters.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action must not be empty.")
            .Must(action => action == NotificationConstants.DataChangeActions.Create ||
                            action == NotificationConstants.DataChangeActions.Update ||
                            action == NotificationConstants.DataChangeActions.Delete)
            .WithMessage($"Action must be either {NotificationConstants.DataChangeActions.Create}, {NotificationConstants.DataChangeActions.Update}, or {NotificationConstants.DataChangeActions.Delete}.");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("EntityId must not be empty.")
            .MaximumLength(100).WithMessage("EntityId must not exceed 100 characters.");

        RuleFor(x => x.Sensitivity)
            .NotEmpty().WithMessage("Sensitivity must not be empty.")
            .Must(s => s == NotificationConstants.DataSensitivity.Low ||
                       s == NotificationConstants.DataSensitivity.Medium ||
                       s == NotificationConstants.DataSensitivity.High)
            .WithMessage($"Sensitivity must be either {NotificationConstants.DataSensitivity.Low}, {NotificationConstants.DataSensitivity.Medium}, or {NotificationConstants.DataSensitivity.High}.");

        RuleFor(x => x.QueryTags)
            .NotNull().WithMessage("QueryTags list must not be null.")
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("QueryTags cannot exceed 20 items.");

        RuleForEach(x => x.QueryTags)
            .MaximumLength(100).WithMessage("Each QueryTag must not exceed 100 characters.");
    }
}
