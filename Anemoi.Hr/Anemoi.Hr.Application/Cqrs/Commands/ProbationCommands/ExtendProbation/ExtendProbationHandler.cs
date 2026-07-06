using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Probation;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.ExtendProbation;

public sealed class ExtendProbationHandler(
    ISqlRepository<ProbationRecord> probationRepository,
    IUnitOfWork unitOfWork,
    Mappings.ProbationRecordMapper mapper)
    : ICommandHandler<ExtendProbationCommand, OneOf<ProbationRecordDto, ErrorDetailResponse>>
{
    public async Task<OneOf<ProbationRecordDto, ErrorDetailResponse>> Handle(
        ExtendProbationCommand request, CancellationToken cancellationToken)
    {
        var record = await probationRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (record == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ProbationRecordNotFound!);

        record.Extend(request.NewEndDate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.ToDto(record);
    }
}
