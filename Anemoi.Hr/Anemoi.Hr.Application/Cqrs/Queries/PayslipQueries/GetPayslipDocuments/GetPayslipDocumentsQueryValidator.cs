using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocuments;

public sealed class GetPayslipDocumentsQueryValidator : AbstractValidator<GetPayslipDocumentsQuery>
{
    public GetPayslipDocumentsQueryValidator()
    {
        RuleFor(x => x.PayslipId)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.PayslipIdRequired);
    }
}
