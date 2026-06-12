using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CalculateTax;

public sealed record CalculateTaxCommand(
    string EmployeeId,
    string CountryCode,
    string TaxType,
    decimal GrossIncome,
    decimal TaxableIncome,
    string Currency,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    Dictionary<string, decimal> DeductionInputs,
    string SourceModule,
    Guid? SourceReferenceId = null,
    string CalculatedBy = null) : ICommandResult<CalculateTaxResponse>;
