using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.BulkImport.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;
using Anemoi.Hr.Domain.BulkImport;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.EmployeeImport;

public class ExecuteImportHandlerTests
{
    private readonly ISqlRepository<BulkImportJob> _jobRepository;
    private readonly IImportOrchestrator _orchestrator;
    private readonly ExecuteImportHandler _handler;

    public ExecuteImportHandlerTests()
    {
        _jobRepository = Substitute.For<ISqlRepository<BulkImportJob>>();
        _orchestrator = Substitute.For<IImportOrchestrator>();
        _handler = new ExecuteImportHandler(_jobRepository, _orchestrator);
    }

    [Fact]
    public async Task Handle_JobNotFound_ShouldReturnError()
    {
        _jobRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BulkImportJob, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns((BulkImportJob)null);

        var command = new ExecuteImportCommand(new BulkImportJobId(Guid.NewGuid()), "user1", "User");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.ImportJobNotFound);
    }

    [Fact]
    public async Task Handle_ValidJob_ShouldDelegateToOrchestrator()
    {
        var jobId = new BulkImportJobId(Guid.NewGuid());
        var job = new BulkImportJob
        {
            Id = jobId,
            OriginalFileName = "employees.xlsx",
            TotalRows = 5,
            StatusCode = BulkImportJobStatus.Pending
        };

        var orchestratorResult = new EmployeeImportResultResponse(
            jobId, 5, 5, 0, BulkImportJobStatus.Completed, null, null);

        _jobRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BulkImportJob, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(job);

        _orchestrator.ExecuteImportAsync(job, "user1", "User", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<EmployeeImportResultResponse, ErrorDetailResponse>>(orchestratorResult));

        var command = new ExecuteImportCommand(jobId, "user1", "User");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var response = result.AsT0;
        response.TotalRows.Should().Be(5);
        response.ImportedRows.Should().Be(5);
        response.Status.Should().Be(BulkImportJobStatus.Completed);

        await _orchestrator.Received(1).ExecuteImportAsync(
            job, "user1", "User", Arg.Any<CancellationToken>());
    }
}
