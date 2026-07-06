using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.UpdateInsuranceContributionRule;

public sealed record UpdateInsuranceContributionRuleCommand(
    string Id,
    string ContributionType,
    decimal EmployeeRate,
    decimal EmployerRate,
    decimal? CeilingAmount,
    decimal? MinimumAmount,
    string SalaryBasis,
    int SortOrder) : ICommandResult<SuccessResponse>;
