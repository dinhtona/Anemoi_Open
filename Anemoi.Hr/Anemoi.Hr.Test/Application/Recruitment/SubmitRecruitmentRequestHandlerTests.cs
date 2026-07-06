using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Test.Helpers;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Recruitment;

public class SubmitRecruitmentRequestHandlerTests
{
    private readonly ISqlRepository<RecruitmentRequest> _requestRepository;
    private readonly ISqlRepository<RecruitmentRequestHistory> _historyRepository;
    private readonly ISqlRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ICurrentUser _currentUser;
    private readonly RecruitmentMapper _mapper;
    private readonly SubmitRecruitmentRequestHandler _handler;

    public SubmitRecruitmentRequestHandlerTests()
    {
        _requestRepository = Substitute.For<ISqlRepository<RecruitmentRequest>>();
        _historyRepository = Substitute.For<ISqlRepository<RecruitmentRequestHistory>>();
        _employeeRepository = Substitute.For<ISqlRepository<Employee>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _workflowEngine = Substitute.For<IWorkflowEngine>();
        _currentUser = Substitute.For<ICurrentUser>();
        _mapper = new RecruitmentMapper();
        _handler = new SubmitRecruitmentRequestHandler(
            _requestRepository, _historyRepository, _employeeRepository,
            _unitOfWork, _publishEndpoint, _workflowEngine,
            _currentUser, _mapper);
    }

    private static RecruitmentRequest CreateDraftRequest()
    {
        return new RecruitmentRequest
        {
            Id = new RecruitmentRequestId(Guid.NewGuid()),
            RequestNumber = "REQ-001",
            DepartmentId = new DepartmentId(Guid.NewGuid()),
            PositionId = new PositionId(Guid.NewGuid()),
            RequestedHeadcount = 2,
            Reason = "Team expansion",
            PriorityCode = "Normal",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handle_WorkflowStartFails_ShouldReturnErrorAndNotSaveOrPublish()
    {
        var draftRequest = CreateDraftRequest();
        var userId = Guid.NewGuid().ToString();

        _currentUser.UserId.Returns(userId);

        _requestRepository.GetQueryable()
            .Returns(AsyncQueryableHelper.CreateMockQueryable(new[] { draftRequest }));

        _historyRepository.CreateOneAsync(
                Arg.Any<RecruitmentRequestHistory>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var history = callInfo.Arg<RecruitmentRequestHistory>();
                return Task.FromResult<OneOf<RecruitmentRequestHistory, Exception>>(history);
            });

        var employee = new Employee
        {
            Id = new EmployeeId(Guid.NewGuid()),
            DisplayName = "Test Employee",
            EmployeeCode = "EMP-001",
            IdentityUserId = Guid.Parse(userId)
        };
        _employeeRepository.GetQueryable()
            .Returns(AsyncQueryableHelper.CreateMockQueryable(new[] { employee }));

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

        var command = new SubmitRecruitmentRequestCommand(
            draftRequest.Id.Value.ToString(), userId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be("HR_WORKFLOW_APPROVER_NOT_FOUND");

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
        await _publishEndpoint.DidNotReceive()
            .Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }
}
