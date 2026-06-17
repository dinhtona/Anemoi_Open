using System;
using System.Linq;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CompleteOnboardingTask;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using Serilog;

namespace Anemoi.Hr.Application.Services;

public sealed class OnboardingNotificationActionHandler(
    IMediator mediator,
    ILogger logger) : INotificationActionHandler
{
    public string TargetService => NotificationWorkflowConstants.TargetServices.Onboarding;

    public async Task<NotificationActionResult> HandleAsync(NotificationActionCommand cmd)
    {
        var completeCmd = new CompleteOnboardingTaskCommand(
            new OnboardingTaskId(Guid.Parse(cmd.AggregateId)),
            cmd.Comment)
        {
            CompletedBy = cmd.UserId
        };

        return cmd.ActionCode switch
        {
            NotificationWorkflowConstants.ActionCodes.ViewOnboardingTask =>
                new NotificationActionResult(true, "Navigating to onboarding task"),
            NotificationWorkflowConstants.ActionCodes.CompleteOnboardingTask =>
                await SendResult<OnboardingInstanceResponse>(completeCmd),
            _ => new NotificationActionResult(false, $"Unknown onboarding action: {cmd.ActionCode}")
        };
    }

    private async Task<NotificationActionResult> SendResult<TResponse>(
        BuildingBlock.Application.Cqrs.Commands.ICommandResult<TResponse> command)
        where TResponse : class
    {
        try
        {
            var result = await mediator.Send(command);
            return result.Match(
                _ => new NotificationActionResult(true, "Action completed successfully"),
                error => new NotificationActionResult(false,
                    string.Join("; ", error.Messages ?? Enumerable.Empty<string>())));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error executing result command {CommandType}", command.GetType().Name);
            return new NotificationActionResult(false, ex.Message);
        }
    }
}
