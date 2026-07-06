using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.EmployeeImport;
using Anemoi.Hr.Domain.BulkImport;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Anemoi.Hr.Test.Application.EmployeeImport;

public class GetImportJobDetailHandlerTests
{
    private readonly ISqlRepository<BulkImportJob> _jobRepository;
    private readonly GetImportJobDetailHandler _handler;

    public GetImportJobDetailHandlerTests()
    {
        _jobRepository = Substitute.For<ISqlRepository<BulkImportJob>>();
        _handler = new GetImportJobDetailHandler(_jobRepository);
    }

    [Fact]
    public async Task Handle_JobNotFound_ShouldReturnError()
    {
        _jobRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BulkImportJob, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns((BulkImportJob)null);

        var query = new GetImportJobDetailQuery(new BulkImportJobId(Guid.NewGuid()));
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.ImportJobNotFound);
    }

    [Fact]
    public async Task Handle_JobExists_ShouldReturnDetail()
    {
        var jobId = new BulkImportJobId(Guid.NewGuid());
        var now = DateTime.UtcNow;
        var job = new BulkImportJob
        {
            Id = jobId,
            EntityType = "Employee",
            OriginalFileName = "employees.xlsx",
            TotalRows = 50,
            ImportedRows = 48,
            FailedRows = 2,
            StatusCode = BulkImportJobStatus.Completed,
            ActorName = "Jane Admin",
            CreatedAt = now,
            CompletedAt = now.AddSeconds(15),
            ExecutionDurationMs = 15000,
            ErrorDetails = "[{\"RowIndex\":3,\"Column\":\"Email\"}]"
        };

        _jobRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BulkImportJob, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(job);

        var query = new GetImportJobDetailQuery(jobId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var detail = result.AsT0;
        detail.OriginalFileName.Should().Be("employees.xlsx");
        detail.TotalRows.Should().Be(50);
        detail.ImportedRows.Should().Be(48);
        detail.FailedRows.Should().Be(2);
        detail.Status.Should().Be(BulkImportJobStatus.Completed);
        detail.ImportedBy.Should().Be("Jane Admin");
        detail.ErrorDetails.Should().NotBeNull();
    }
}
