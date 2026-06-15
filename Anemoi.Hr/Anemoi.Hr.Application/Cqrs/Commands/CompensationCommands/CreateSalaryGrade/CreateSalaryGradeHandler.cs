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
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateSalaryGrade;

public sealed class CreateSalaryGradeHandler(
    ISqlRepository<SalaryGrade> salaryGradeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSalaryGradeCommand, OneOf<CreateSalaryGradeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateSalaryGradeResponse, ErrorDetailResponse>> Handle(
        CreateSalaryGradeCommand request,
        CancellationToken cancellationToken)
    {
        var cleanCode = request.GradeCode.Trim();
        var exists = await salaryGradeRepository.ExistByConditionAsync(
            x => x.GradeCode == cleanCode,
            cancellationToken);

        if (exists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SalaryGradeAlreadyExists);

        var newGrade = new SalaryGrade
        {
            Id = new SalaryGradeId(IdGenerator.NextGuid()),
            GradeCode = cleanCode,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await salaryGradeRepository.CreateOneAsync(newGrade, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new CreateSalaryGradeResponse { SalaryGradeId = newGrade.Id.Value.ToString() };
    }
}
