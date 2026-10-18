using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.PassProbation;

public sealed class PassProbationHandler(
    ISqlRepository<ProbationRecord> probationRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    Mappings.ProbationRecordMapper mapper)
    : ICommandHandler<PassProbationCommand, OneOf<ProbationRecordDto, ErrorDetailResponse>>
{
    public async Task<OneOf<ProbationRecordDto, ErrorDetailResponse>> Handle(
        PassProbationCommand request, CancellationToken cancellationToken)
    {
        var record = await probationRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (record == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ProbationRecordNotFound!);

        Employee reviewer = null;
        if (Guid.TryParse(currentUser.UserId, out var identityUserId))
            reviewer = await employeeRepository.GetFirstByConditionAsync(
                x => x.IdentityUserId == identityUserId, null, cancellationToken);
        if (reviewer == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        record.Pass(request.Result, request.Comment, reviewer.Id);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToDto(record);
    }
}
