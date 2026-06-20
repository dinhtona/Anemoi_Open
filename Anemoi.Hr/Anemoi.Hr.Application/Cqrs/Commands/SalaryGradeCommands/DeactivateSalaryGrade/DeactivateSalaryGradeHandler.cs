using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.SalaryGradeCommands.DeactivateSalaryGrade;

public sealed class DeactivateSalaryGradeHandler(
    ISqlRepository<SalaryGrade> salaryGradeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateSalaryGradeCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeactivateSalaryGradeCommand request,
        CancellationToken cancellationToken)
    {
        var salaryGrade = await salaryGradeRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (salaryGrade is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SalaryGradeNotFound);

        salaryGrade.IsActive = false;
        salaryGrade.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return new SuccessResponse();
    }
}
