using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateSalaryRange;

public sealed class CreateSalaryRangeHandler(
    ISqlRepository<SalaryGrade> salaryGradeRepository,
    ISqlRepository<SalaryRange> salaryRangeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSalaryRangeCommand, OneOf<CreateSalaryRangeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateSalaryRangeResponse, ErrorDetailResponse>> Handle(
        CreateSalaryRangeCommand request,
        CancellationToken cancellationToken)
    {
        var gradeExists = await salaryGradeRepository.ExistByConditionAsync(
            x => x.Id == request.SalaryGradeId,
            cancellationToken);

        if (!gradeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SalaryGradeNotFound);

        // Validate range overlap for the same Grade + Currency
        var ranges = await salaryRangeRepository.GetManyByConditionAsync(
            x => x.SalaryGradeId == request.SalaryGradeId && 
                 x.Currency == request.Currency && 
                 x.IsActive,
            null,
            cancellationToken);

        var hasOverlap = ranges.Any(r => 
            (request.EffectiveTo == null || r.EffectiveFrom <= request.EffectiveTo) && 
            (r.EffectiveTo == null || r.EffectiveTo >= request.EffectiveFrom));

        if (hasOverlap)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SalaryRangeOverlapping);

        var newRange = new SalaryRange
        {
            Id = new SalaryRangeId(IdGenerator.NextGuid()),
            SalaryGradeId = request.SalaryGradeId,
            MinSalary = request.MinSalary,
            MaxSalary = request.MaxSalary,
            Currency = request.Currency.Trim(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await salaryRangeRepository.CreateOneAsync(newRange, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new CreateSalaryRangeResponse { SalaryRangeId = newRange.Id.Value.ToString() };
    }
}
