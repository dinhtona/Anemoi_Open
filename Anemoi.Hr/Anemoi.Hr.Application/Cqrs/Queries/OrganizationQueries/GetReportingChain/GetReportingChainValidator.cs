using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetReportingChain;

public sealed class GetReportingChainValidator : AbstractValidator<GetReportingChainQuery>
{
    public GetReportingChainValidator()
    {
        RuleFor(x => x.EmployeeId).RequiredId(HrBusinessErrorCodes.ValEmployeeIdRequired);
    }
}
