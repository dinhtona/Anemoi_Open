using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.SalaryGradeCommands.ActivateSalaryGrade;

public sealed class ActivateSalaryGradeHandler(
    ISqlRepository<SalaryGrade> salaryGradeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivateSalaryGradeCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        ActivateSalaryGradeCommand request,
        CancellationToken cancellationToken)
    {
        var salaryGrade = await salaryGradeRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (salaryGrade is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SalaryGradeNotFound);

        salaryGrade.IsActive = true;
        salaryGrade.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return new SuccessResponse();
    }
}
