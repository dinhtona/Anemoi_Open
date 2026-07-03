using System.Text;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.BulkImport.Services;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;
using Anemoi.Hr.Domain.BulkImport;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.EmployeeImport;

public class UploadImportFileHandlerTests
{
    private readonly ISqlRepository<BulkImportJob> _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BulkExcelParserService _parser;

    public UploadImportFileHandlerTests()
    {
        _jobRepository = Substitute.For<ISqlRepository<BulkImportJob>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _parser = new BulkExcelParserService();
    }

    private static byte[] CreateCsvBytes(string header, params string[] rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var row in rows)
            sb.AppendLine(row);
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    [Fact]
    public async Task Handle_InvalidFileExtension_ShouldReturnError()
    {
        var handler = CreateHandler();
        var command = new UploadImportFileCommand("data.pdf", 1024, []);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.InvalidFileFormat);
    }

    [Fact]
    public async Task Handle_FileTooLarge_ShouldReturnError()
    {
        var handler = CreateHandler();
        var command = new UploadImportFileCommand(
            "data.xlsx", BulkImportConstants.MaxFileSizeBytes + 1, []);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.FileTooLarge);
    }

    [Fact]
    public async Task Handle_EmptyCsvFile_ShouldReturnError()
    {
        var handler = CreateHandler();
        var bytes = CreateCsvBytes("EmployeeCode,FirstName");
        var command = new UploadImportFileCommand("data.csv", bytes.Length, bytes);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.ImportFileEmpty);
    }

    [Fact]
    public async Task Handle_ParseFailure_ShouldReturnError()
    {
        var handler = CreateHandler();
        var command = new UploadImportFileCommand("data.xlsx", 1024, [0xFF, 0xD8, 0xFF]);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.FileParseFailed);
    }

    [Fact]
    public async Task Handle_TooManyRows_ShouldReturnError()
    {
        var header = string.Join(",", Enumerable.Range(0, 5).Select(i => $"Col{i}"));
        var rows = Enumerable.Range(0, BulkImportConstants.MaxUploadRows + 1)
            .Select(i => $"val{i},a,b,c,d")
            .ToArray();
        var bytes = CreateCsvBytes(header, rows);
        var command = new UploadImportFileCommand("data.csv", bytes.Length, bytes);

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.ImportTooManyRows);
    }

    private UploadImportFileHandler CreateHandler()
    {
        return new UploadImportFileHandler(
            _parser, null!, null!, null!, null!, _jobRepository, _unitOfWork);
    }
}
