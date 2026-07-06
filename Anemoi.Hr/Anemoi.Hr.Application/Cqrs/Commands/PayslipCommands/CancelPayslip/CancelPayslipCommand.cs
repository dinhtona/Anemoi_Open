using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.CancelPayslip;

public sealed record CancelPayslipCommand(
    PayslipId PayslipId,
    [property: JsonIgnore] string CancelledBy = null) : ICommandResult<PayslipResponse>;
