using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Probation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.PassProbation;

public sealed class PassProbationHandler(
    ISqlRepository<ProbationRecord> probationRepository,
    IUnitOfWork unitOfWork,
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

        record.Pass(request.Result, request.Comment, new EmployeeId(Guid.Empty));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.ToDto(record);
    }
}
