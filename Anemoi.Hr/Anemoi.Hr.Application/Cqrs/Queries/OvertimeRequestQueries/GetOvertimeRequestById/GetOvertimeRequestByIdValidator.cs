using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Validators;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRequestQueries.GetOvertimeRequestById;

public sealed class GetOvertimeRequestByIdValidator : AbstractValidator<GetOvertimeRequestByIdQuery>
{
    public GetOvertimeRequestByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .WithErrorCode(HrBusinessErrorCodes.OvertimeRequestNotFound);
    }
}
