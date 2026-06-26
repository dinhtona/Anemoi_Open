using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Cqrs.Commands.PayrollCommands.SubmitPayrollRunForApproval;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Payroll;

public class SubmitPayrollRunForApprovalHandlerTests
{
    private readonly ISqlRepository<PayrollRun> _payrollRunRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly PayrollMapper _mapper;
    private readonly SubmitPayrollRunForApprovalHandler _handler;

    public SubmitPayrollRunForApprovalHandlerTests()
    {
        _payrollRunRepository = Substitute.For<ISqlRepository<PayrollRun>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _workflowEngine = Substitute.For<IWorkflowEngine>();
        _mapper = new PayrollMapper();
        _handler = new SubmitPayrollRunForApprovalHandler(
            _payrollRunRepository, _unitOfWork, _publishEndpoint,
            _workflowEngine, _mapper);
    }

    private static PayrollRun CreateCalculatedRun()
    {
        return new PayrollRun
        {
            Id = new PayrollRunId(Guid.NewGuid()),
            PayrollPeriodId = new PayrollPeriodId(Guid.NewGuid()),
            EmployeeId = new EmployeeId(Guid.NewGuid()),
            EmployeeCode = "EMP-001",
            EmployeeName = "Test Employee",
            BaseSalary = 5000m,
            CurrencyCode = "USD",
            PayScheduleType = "Monthly",
            StandardWorkingDays = 22,
            PaidWorkingDays = 22,
            UnpaidLeaveDays = 0,
            DailyRate = 227.27m,
            BasePayAmount = 5000m,
            TotalAllowanceAmount = 500m,
            GrossAmount = 5500m,
            TotalDeductionAmount = 1000m,
            NetAmount = 4500m,
            CalculatedAt = DateTime.UtcNow,
            CalculatedBy = "system"
        };
    }

    [Fact]
    public async Task Handle_WorkflowStartFails_ShouldReturnErrorAndNotSaveOrPublish()
    {
        var run = CreateCalculatedRun();
        var submittedBy = Guid.NewGuid().ToString();

        _payrollRunRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<PayrollRun, bool>>>(),
                Arg.Any<Func<IQueryable<PayrollRun>, IQueryable<PayrollRun>>>(),
                Arg.Any<CancellationToken>())
            .Returns(run);

        var workflowError = new ErrorDetailResponse
        {
            Code = "HR_WORKFLOW_APPROVER_NOT_FOUND"
        };

        _workflowEngine.StartAsync(
                Arg.Any<string>(),
                Arg.Any<Guid>(),
                Arg.Any<EmployeeId>(),
                Arg.Any<UserId>(),
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<WorkflowInstance, ErrorDetailResponse>>(workflowError));

        var command = new SubmitPayrollRunForApprovalCommand(
            run.Id, submittedBy);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be("HR_WORKFLOW_APPROVER_NOT_FOUND");

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
        await _publishEndpoint.DidNotReceive()
            .Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
