using System;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr;
using Anemoi.Notification.Domain.Models;
using MassTransit;
using OneOf;

namespace Anemoi.Notification.Application.Services;

public sealed class LeaveNotificationExecutor(IRequestClient<NotificationActionCommand> requestClient)
    : INotificationActionExecutor
{
    public string ActionCode => NotificationWorkflowConstants.TargetServices.Leave;

    public async Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
        NotificationHistory notification, NotificationAction action, CancellationToken cancellationToken)
    {
        if (action.ActionType == "Navigate")
        {
            var targetUrl = $"/hr/leave/requests/{notification.AggregateId}";
            return new ExecuteActionResult("Navigate", targetUrl, true, "Navigating to leave request");
        }

        return await SendAction(notification, action, cancellationToken);
    }

    private async Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> SendAction(
        NotificationHistory notification, NotificationAction action, CancellationToken ct)
    {
        try
        {
            var cmd = new NotificationActionCommand(
                NotificationWorkflowConstants.TargetServices.Leave, action.ActionCode, notification.AggregateId,
                notification.UserId.ToString(), null);

            var response = await requestClient.GetResponse<NotificationActionResult>(cmd, ct);
            var result = response.Message;

            return new ExecuteActionResult("Command", null, result.Success, result.Message);
        }
        catch (Exception)
        {
            return new ExecuteActionResult("Command", null, false, "Failed to execute leave action");
        }
    }
}

public sealed class OvertimeNotificationExecutor(IRequestClient<NotificationActionCommand> requestClient)
    : INotificationActionExecutor
{
    public string ActionCode => NotificationWorkflowConstants.TargetServices.Overtime;

    public async Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
        NotificationHistory notification, NotificationAction action, CancellationToken cancellationToken)
    {
        if (action.ActionType == "Navigate")
        {
            var targetUrl = $"/hr/overtime/{notification.AggregateId}";
            return new ExecuteActionResult("Navigate", targetUrl, true, "Navigating to overtime request");
        }

        try
        {
            var cmd = new NotificationActionCommand(
                NotificationWorkflowConstants.TargetServices.Overtime, action.ActionCode, notification.AggregateId,
                notification.UserId.ToString(), null);

            var response = await requestClient.GetResponse<NotificationActionResult>(cmd, cancellationToken);
            var result = response.Message;

            return new ExecuteActionResult("Command", null, result.Success, result.Message);
        }
        catch (Exception)
        {
            return new ExecuteActionResult("Command", null, false, "Failed to execute overtime action");
        }
    }
}

public sealed class PayrollNotificationExecutor(IRequestClient<NotificationActionCommand> requestClient)
    : INotificationActionExecutor
{
    public string ActionCode => NotificationWorkflowConstants.TargetServices.Payroll;

    public async Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
        NotificationHistory notification, NotificationAction action, CancellationToken cancellationToken)
    {
        if (action.ActionType == "Navigate")
        {
            var targetUrl = $"/hr/payroll/runs/{notification.AggregateId}";
            return new ExecuteActionResult("Navigate", targetUrl, true, "Navigating to payroll run");
        }

        try
        {
            var cmd = new NotificationActionCommand(
                NotificationWorkflowConstants.TargetServices.Payroll, action.ActionCode, notification.AggregateId,
                notification.UserId.ToString(), null);

            var response = await requestClient.GetResponse<NotificationActionResult>(cmd, cancellationToken);
            var result = response.Message;

            return new ExecuteActionResult("Command", null, result.Success, result.Message);
        }
        catch (Exception)
        {
            return new ExecuteActionResult("Command", null, false, "Failed to execute payroll action");
        }
    }
}

public sealed class OnboardingNotificationExecutor(IRequestClient<NotificationActionCommand> requestClient)
    : INotificationActionExecutor
{
    public string ActionCode => NotificationWorkflowConstants.TargetServices.Onboarding;

    public async Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
        NotificationHistory notification, NotificationAction action, CancellationToken cancellationToken)
    {
        if (action.ActionType == "Navigate")
        {
            var targetUrl = $"/hr/onboarding/tasks/{notification.AggregateId}";
            return new ExecuteActionResult("Navigate", targetUrl, true, "Navigating to onboarding task");
        }

        try
        {
            var cmd = new NotificationActionCommand(
                NotificationWorkflowConstants.TargetServices.Onboarding, action.ActionCode, notification.AggregateId,
                notification.UserId.ToString(), null);

            var response = await requestClient.GetResponse<NotificationActionResult>(cmd, cancellationToken);
            var result = response.Message;

            return new ExecuteActionResult("Command", null, result.Success, result.Message);
        }
        catch (Exception)
        {
            return new ExecuteActionResult("Command", null, false, "Failed to execute onboarding action");
        }
    }
}

public sealed class RecruitmentNotificationExecutor : INotificationActionExecutor
{
    public string ActionCode => NotificationWorkflowConstants.TargetServices.Recruitment;

    public Task<OneOf<ExecuteActionResult, ErrorDetailResponse>> ExecuteAsync(
        NotificationHistory notification, NotificationAction action, CancellationToken cancellationToken)
    {
        var targetUrl = $"/hr/recruitment/requests/{notification.AggregateId}";
        var result = new ExecuteActionResult("Navigate", targetUrl, true, "Navigating to recruitment request");
        return Task.FromResult<OneOf<ExecuteActionResult, ErrorDetailResponse>>(result);
    }
}
