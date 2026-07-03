using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.BulkImport.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.BulkImport;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed class ExecuteImportHandler(
    ISqlRepository<BulkImportJob> jobRepository,
    IImportOrchestrator orchestrator)
    : ICommandHandler<ExecuteImportCommand, OneOf<EmployeeImportResultResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeImportResultResponse, ErrorDetailResponse>> Handle(
        ExecuteImportCommand request, CancellationToken ct)
    {
        var job = await jobRepository.GetFirstByConditionAsync(
            j => j.Id == request.JobId, null, ct);
        if (job is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ImportJobNotFound);

        return await orchestrator.ExecuteImportAsync(
            job, request.ActorUserId ?? "", request.ActorName, ct);
    }
}
