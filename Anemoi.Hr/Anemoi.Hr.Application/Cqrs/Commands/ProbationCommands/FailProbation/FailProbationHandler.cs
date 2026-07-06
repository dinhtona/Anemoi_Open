using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Probation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.FailProbation;

public sealed class FailProbationHandler(
    ISqlRepository<ProbationRecord> probationRepository,
    IUnitOfWork unitOfWork,
    Mappings.ProbationRecordMapper mapper)
    : ICommandHandler<FailProbationCommand, OneOf<ProbationRecordDto, ErrorDetailResponse>>
{
    public async Task<OneOf<ProbationRecordDto, ErrorDetailResponse>> Handle(
        FailProbationCommand request, CancellationToken cancellationToken)
    {
        var record = await probationRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (record == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ProbationRecordNotFound!);

        record.Fail(request.Comment, new EmployeeId(Guid.Empty));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.ToDto(record);
    }
}
