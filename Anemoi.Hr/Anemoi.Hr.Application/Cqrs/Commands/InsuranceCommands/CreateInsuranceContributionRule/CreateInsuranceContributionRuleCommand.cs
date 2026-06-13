using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceContributionRule;

public sealed record CreateInsuranceContributionRuleCommand(
    string RuleSetId,
    string ContributionType,
    decimal EmployeeRate,
    decimal EmployerRate,
    decimal? CeilingAmount,
    decimal? MinimumAmount,
    string SalaryBasis,
    int SortOrder) : ICommandResult<CreateInsuranceContributionRuleResponse>;
