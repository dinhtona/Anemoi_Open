using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.PublishPayslip;

public sealed record PublishPayslipCommand(
    PayslipId PayslipId,
    [property: JsonIgnore] string PublishedBy = null) : ICommandResult<PayslipResponse>;
