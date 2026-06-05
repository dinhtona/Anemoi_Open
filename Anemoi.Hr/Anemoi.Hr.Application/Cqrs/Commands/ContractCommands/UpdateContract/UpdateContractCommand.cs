using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.UpdateContract;

public sealed record UpdateContractCommand(
    EmployeeContractId Id,
    string ContractNumber,
    string ContractTypeCode,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly SignedDate,
    string Notes,
    string AttachmentFileId = null,
    [property: JsonIgnore] string UpdatedBy = null) : ICommandResult<EmployeeContractDetailResponse>;
