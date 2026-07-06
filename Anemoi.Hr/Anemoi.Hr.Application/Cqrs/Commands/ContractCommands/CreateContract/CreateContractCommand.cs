using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.ContractCommands.CreateContract;

public sealed record CreateContractCommand(
    EmployeeId EmployeeId,
    string ContractNumber,
    string ContractTypeCode,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly SignedDate,
    string Notes,
    string AttachmentFileId = null,
    Guid? PreviousContractId = null,
    bool IsDraft = false,
    [property: JsonIgnore] string CreatedBy = null) : ICommandResult<EmployeeContractDetailResponse>;
