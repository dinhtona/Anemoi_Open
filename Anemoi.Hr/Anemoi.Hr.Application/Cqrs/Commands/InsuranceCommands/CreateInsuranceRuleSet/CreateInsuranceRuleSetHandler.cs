using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.InsuranceCommands.CreateInsuranceRuleSet;

public sealed class CreateInsuranceRuleSetHandler(
    ISqlRepository<InsuranceRuleSet> ruleSetRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateInsuranceRuleSetCommand, OneOf<CreateInsuranceRuleSetResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateInsuranceRuleSetResponse, ErrorDetailResponse>> Handle(
        CreateInsuranceRuleSetCommand request,
        CancellationToken cancellationToken)
    {
        var countryCode = request.CountryCode.Trim().ToUpperInvariant();
        var insuranceType = request.InsuranceType.Trim().ToUpperInvariant();

        var existingSets = await ruleSetRepository.GetManyByConditionAsync(
            x => x.CountryCode == countryCode && x.InsuranceType == insuranceType && x.Status != InsuranceRuleSetStatuses.Inactive,
            token: cancellationToken);

        var maxVersion = existingSets.Select(x => x.Version).DefaultIfEmpty(0).Max();

        var newRuleSet = new InsuranceRuleSet
        {
            Id = new InsuranceRuleSetId(IdGenerator.NextGuid()),
            CountryCode = countryCode,
            InsuranceType = insuranceType,
            Name = request.Name.Trim(),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Status = InsuranceRuleSetStatuses.Draft,
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

        return new CreateInsuranceRuleSetResponse { InsuranceRuleSetId = newRuleSet.Id.Value.ToString() };
    }
}
