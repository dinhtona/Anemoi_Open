using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Taxation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxRuleSet;

public sealed class CreateTaxRuleSetHandler(
    ISqlRepository<TaxRuleSet> ruleSetRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTaxRuleSetCommand, OneOf<CreateTaxRuleSetResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateTaxRuleSetResponse, ErrorDetailResponse>> Handle(
        CreateTaxRuleSetCommand request,
        CancellationToken cancellationToken)
    {
        var countryCode = request.CountryCode.Trim().ToUpperInvariant();
        var taxType = request.TaxType.Trim().ToUpperInvariant();
        var effectiveFrom = request.EffectiveFrom;
        var effectiveTo = request.EffectiveTo;

        if (effectiveTo.HasValue && effectiveFrom > effectiveTo.Value)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetInvalidDateRange);
        }

        // Check for overlapping rule sets (only those that are Active or Draft, not Inactive)
        var existingSets = await ruleSetRepository.GetManyByConditionAsync(
            x => x.CountryCode == countryCode && x.TaxType == taxType && x.Status != TaxRuleSetStatusCode.Inactive,
            token: cancellationToken);

        var overlaps = existingSets.Any(x =>
            effectiveFrom <= (x.EffectiveTo ?? DateOnly.MaxValue) &&
            (effectiveTo ?? DateOnly.MaxValue) >= x.EffectiveFrom);

        if (overlaps)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.TaxRuleSetOverlappingPeriod);
        }

        // Determine next version for CountryCode + TaxType
        var maxVersion = existingSets.Select(x => x.Version).DefaultIfEmpty(0).Max();

        var newRuleSet = new TaxRuleSet
        {
            Id = new TaxRuleSetId(IdGenerator.NextGuid()),
            CountryCode = countryCode,
            TaxType = taxType,
            Name = request.Name.Trim(),
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            Status = TaxRuleSetStatusCode.Draft,
            Version = maxVersion + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy ?? PayrollConstants.SystemActor,
            UpdatedBy = request.CreatedBy ?? PayrollConstants.SystemActor
        };

        await ruleSetRepository.CreateOneAsync(newRuleSet, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new CreateTaxRuleSetResponse { TaxRuleSetId = newRuleSet.Id.Value.ToString() };
    }
}
