using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipsForPayrollRun;

public sealed record GeneratePayslipsForPayrollRunCommand(
    PayrollRunId PayrollRunId,
    [property: JsonIgnore] string GeneratedBy = null) : ICommandResult<IReadOnlyCollection<PayslipResponse>>;
