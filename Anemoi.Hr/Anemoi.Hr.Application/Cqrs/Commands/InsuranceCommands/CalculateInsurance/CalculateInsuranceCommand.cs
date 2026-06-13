using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CalculateInsurance;

public sealed record CalculateInsuranceCommand(
    string EmployeeId,
    string CountryCode,
    string InsuranceType,
    decimal GrossSalarySnapshot,
    decimal? ContractSalarySnapshot,
    string Currency,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string SourceModule,
    string SourceReferenceId = null,
    string CalculatedBy = null) : ICommandResult<CalculateInsuranceResponse>;
