using Anemoi.BuildingBlock.Domain;
using FluentValidation;

namespace Anemoi.Hr.Application.Validators;

public static class HrValidationRules
{
    public static IRuleBuilderOptions<T, TId> RequiredId<T, TId>(this IRuleBuilder<T, TId> ruleBuilder, string errorCode)
        where TId : StronglyTypedId<Guid>
    {
        return ruleBuilder
            .Must(x => x is { } && x.Value != Guid.Empty)
            .WithMessage(errorCode);
    }
}
