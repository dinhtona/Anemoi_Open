using System;
using System.Threading.Tasks;
using Anemoi.Contract.Hr;
using Anemoi.Hr.Application.Services;
using MassTransit;
using Serilog;

namespace Anemoi.Hr.Application.Consumers;

public sealed class NotificationActionCommandConsumer(
    INotificationActionHandlerRegistry handlerRegistry,
    ILogger logger) : IConsumer<NotificationActionCommand>
{
    public async Task Consume(ConsumeContext<NotificationActionCommand> context)
    {
        var cmd = context.Message;
        try
        {
            var handler = handlerRegistry.GetHandler(cmd.TargetService);
            if (handler == null)
            {
                await context.RespondAsync(
                    new NotificationActionResult(false, $"Unknown target service: {cmd.TargetService}"));
                return;
            }

            var result = await handler.HandleAsync(cmd);
            await context.RespondAsync(result);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error handling notification action for {TargetService}/{ActionCode}",
                cmd.TargetService, cmd.ActionCode);
            await context.RespondAsync(new NotificationActionResult(false, "Internal error"));
        }
    }
}
