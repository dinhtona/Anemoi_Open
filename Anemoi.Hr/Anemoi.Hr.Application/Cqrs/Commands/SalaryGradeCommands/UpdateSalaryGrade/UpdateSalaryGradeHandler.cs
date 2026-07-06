using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.SalaryGradeCommands.UpdateSalaryGrade;

public sealed class UpdateSalaryGradeHandler(
    ISqlRepository<SalaryGrade> salaryGradeRepository,
    IUnitOfWork unitOfWork,
    CompensationMapper mapper)
    : ICommandHandler<UpdateSalaryGradeCommand, OneOf<SalaryGradeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SalaryGradeResponse, ErrorDetailResponse>> Handle(
        UpdateSalaryGradeCommand request,
        CancellationToken cancellationToken)
    {
        var salaryGrade = await salaryGradeRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (salaryGrade is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SalaryGradeNotFound);

        var cleanCode = request.GradeCode.Trim();
        var codeExists = await salaryGradeRepository.ExistByConditionAsync(
            x => x.GradeCode == cleanCode && x.Id != request.Id,
            cancellationToken);

        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SalaryGradeAlreadyExists);

        salaryGrade.GradeCode = cleanCode;
        salaryGrade.Name = request.Name.Trim();
        salaryGrade.Description = request.Description?.Trim();
        salaryGrade.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToSalaryGradeResponse(salaryGrade);
    }
}
