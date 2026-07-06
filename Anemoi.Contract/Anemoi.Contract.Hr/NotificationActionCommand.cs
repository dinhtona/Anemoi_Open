namespace Anemoi.Contract.Hr;

public sealed record NotificationActionCommand(
    string TargetService,
    string ActionCode,
    string AggregateId,
    string UserId,
    string? Comment
);

public sealed record NotificationActionResult(
    bool Success,
    string Message
);
