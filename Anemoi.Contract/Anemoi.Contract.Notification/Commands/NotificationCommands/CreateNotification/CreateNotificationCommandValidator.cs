using System;
using Anemoi.Contract.Notification.Constants;
using FluentValidation;

namespace Anemoi.Contract.Notification.Commands.NotificationCommands.CreateNotification;

public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .Must(x => Guid.TryParse(x, out _))
            .WithMessage("UserId must be a valid GUID.");

        RuleFor(x => x.WorkspaceId)
            .Must(x => x is null || Guid.TryParse(x, out _))
            .When(x => x.WorkspaceId is not null)
            .WithMessage("WorkspaceId must be a valid GUID when provided.");

        RuleFor(x => x.Title)
            .NotEmpty().When(x => string.IsNullOrEmpty(x.TitleLocalizationKey))
            .WithMessage("Title is required when TitleLocalizationKey is not specified.")
            .MaximumLength(256);

        RuleFor(x => x.TitleLocalizationKey)
            .NotEmpty().When(x => string.IsNullOrEmpty(x.Title))
            .WithMessage("TitleLocalizationKey is required when Title is not specified.")
            .MaximumLength(256);

        RuleFor(x => x.Content)
            .NotEmpty().When(x => string.IsNullOrEmpty(x.ContentLocalizationKey))
            .WithMessage("Content is required when ContentLocalizationKey is not specified.")
            .MaximumLength(2048);

        RuleFor(x => x.ContentLocalizationKey)
            .NotEmpty().When(x => string.IsNullOrEmpty(x.Content))
            .WithMessage("ContentLocalizationKey is required when Content is not specified.")
            .MaximumLength(256);

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(x => NotificationConstants.Categories.AllowedCategories.Contains(x))
            .WithMessage("Category must be one of the allowed categories.");
    }
}
