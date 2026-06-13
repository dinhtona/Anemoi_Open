using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload;

public sealed class GetPayslipDocumentDownloadQueryValidator : AbstractValidator<GetPayslipDocumentDownloadQuery>
{
    public GetPayslipDocumentDownloadQueryValidator()
    {
        RuleFor(x => x.PayslipId)
            .NotEmpty()
            .WithMessage("PayslipId must not be empty.");

        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("DocumentId must not be empty.");
    }
}
