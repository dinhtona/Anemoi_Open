using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload;

public sealed class GetPayslipDocumentDownloadQueryValidator : AbstractValidator<GetPayslipDocumentDownloadQuery>
{
    public GetPayslipDocumentDownloadQueryValidator()
    {
        RuleFor(x => x.PayslipId)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.PayslipIdRequired);

        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage(HrBusinessErrorCodes.DocumentIdRequired);
    }
}
