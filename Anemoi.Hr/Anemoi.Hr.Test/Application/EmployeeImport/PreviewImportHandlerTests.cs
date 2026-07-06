using System.Text.Json;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;
using Anemoi.Hr.Domain.BulkImport;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Anemoi.Hr.Test.Application.EmployeeImport;

public class PreviewImportHandlerTests
{
    private readonly ISqlRepository<BulkImportJob> _jobRepository;
    private readonly PreviewImportHandler _handler;

    public PreviewImportHandlerTests()
    {
        _jobRepository = Substitute.For<ISqlRepository<BulkImportJob>>();
        _handler = new PreviewImportHandler(_jobRepository);
    }

    [Fact]
    public async Task Handle_JobNotFound_ShouldReturnError()
    {
        _jobRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BulkImportJob, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns((BulkImportJob)null);

        var command = new PreviewImportCommand(new BulkImportJobId(Guid.NewGuid()));
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.ImportJobNotFound);
    }

    [Fact]
    public async Task Handle_ValidJob_ShouldReturnPreview()
    {
        var previewData = new ImportPreviewData(
            "test.xlsx", 5, 3, 1, 1,
            new List<ImportPreviewRow>
            {
                new(0, new Dictionary<string, string> { ["EmployeeCode"] = "EMP001" },
                    "Valid", null),
                new(1, new Dictionary<string, string> { ["EmployeeCode"] = "EMP002" },
                    "Warning", new List<BulkImportJobError>
                    {
                        new(1, "Department", "", "Department not found", "Warning")
                    }),
                new(2, new Dictionary<string, string> { ["EmployeeCode"] = "EMP003" },
                    "Error", new List<BulkImportJobError>
                    {
                        new(2, "FirstName", "", "First name required", "Error")
                    })
            },
            new List<BulkImportJobError>
            {
                new(2, "FirstName", "", "First name required", "Error")
            });

        var job = new BulkImportJob
        {
            Id = new BulkImportJobId(Guid.NewGuid()),
            PreviewDataJson = JsonSerializer.Serialize(previewData),
            StatusCode = BulkImportJobStatus.Pending
        };

        _jobRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BulkImportJob, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(job);

        var command = new PreviewImportCommand(job.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var preview = result.AsT0;
        preview.TotalRows.Should().Be(5);
        preview.ValidRows.Should().Be(3);
        preview.WarningCount.Should().Be(1);
        preview.ErrorCount.Should().Be(1);
        preview.RowResults.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_NullPreviewData_ShouldReturnError()
    {
        var job = new BulkImportJob
        {
            Id = new BulkImportJobId(Guid.NewGuid()),
            PreviewDataJson = "null",
            StatusCode = BulkImportJobStatus.Pending
        };

        _jobRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BulkImportJob, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(job);

        var command = new PreviewImportCommand(job.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.ImportFileEmpty);
    }
}
