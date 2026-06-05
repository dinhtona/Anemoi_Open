using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.TerminateContract;

public sealed record TerminateContractCommand(
    EmployeeContractId Id,
    DateOnly TerminationDate,
    string ReasonCode,
    string Notes,
    string TerminationAttachmentId = null,
    [property: JsonIgnore] string UpdatedBy = null) : ICommandResult<EmployeeContractDetailResponse>;
