using Anemoi.Hr.Application.Responses;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmail;

public sealed record SendPayslipEmailCommand(
    Guid PayslipId,
    string? SentBy = null,
    string? ToEmail = null) : ICommandResult<PayslipEmailDeliveryResponse>;
