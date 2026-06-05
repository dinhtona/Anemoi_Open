using Anemoi.Hr.Application.Validators;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployee;

public sealed class GetEmployeeValidator : AbstractValidator<GetEmployeeQuery>
{
    public GetEmployeeValidator()
    {
        RuleFor(x => x.Id).RequiredId("VAL_EMPLOYEE_ID_REQUIRED");
    }
}
