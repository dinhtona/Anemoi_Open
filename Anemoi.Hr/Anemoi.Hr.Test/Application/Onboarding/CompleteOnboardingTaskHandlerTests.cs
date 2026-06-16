using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CompleteOnboardingTask;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Onboarding;

public class CompleteOnboardingTaskHandlerTests
{
    private readonly ISqlRepository<OnboardingInstance> _instanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OnboardingMapper _mapper;
    private readonly CompleteOnboardingTaskHandler _handler;

    public CompleteOnboardingTaskHandlerTests()
    {
        _instanceRepository = Substitute.For<ISqlRepository<OnboardingInstance>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = new OnboardingMapper();
        _handler = new CompleteOnboardingTaskHandler(_instanceRepository, _unitOfWork, _mapper);
    }

    private (OnboardingInstance Instance, OnboardingTaskId TaskId) CreateInstanceWithPendingTask()
    {
        var instance = OnboardingInstance.Create(
            new OnboardingInstanceId(Guid.NewGuid()),
            new EmployeeId(Guid.NewGuid()),
            new OnboardingPlanTemplateId(Guid.NewGuid()),
            "Standard Onboarding", 1, DateTime.UtcNow, "user1");

        var taskId = new OnboardingTaskId(Guid.NewGuid());
        var task = OnboardingTask.Create(
            taskId, "Setup workstation", null,
            AssigneeRoleCode.It, "user2", "John Doe",
            "user1", DateTime.UtcNow.AddDays(7), 1);
        instance.AddTask(task);
        return (instance, taskId);
    }

    [Fact]
    public async Task Handle_WithValidTask_ShouldCompleteTask()
    {
        var (instance, taskId) = CreateInstanceWithPendingTask();

        _instanceRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingInstance, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(instance);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<None, Exception>>(new None()));

        var command = new CompleteOnboardingTaskCommand(taskId, "Completed successfully")
        {
            CompletedBy = "user2"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var response = result.AsT0;
        response.Status.Should().Be(OnboardingInstanceStatusCode.Completed);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ShouldReturnError()
    {
        _instanceRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingInstance, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns((OnboardingInstance)null);

        var command = new CompleteOnboardingTaskCommand(new OnboardingTaskId(Guid.NewGuid()), null)
        {
            CompletedBy = "user2"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.HrOnboardingTaskNotFound);
    }

    [Fact]
    public async Task Handle_TaskAlreadyCompleted_ShouldReturnError()
    {
        var instance = OnboardingInstance.Create(
            new OnboardingInstanceId(Guid.NewGuid()),
            new EmployeeId(Guid.NewGuid()),
            new OnboardingPlanTemplateId(Guid.NewGuid()),
            "Standard Onboarding", 1, DateTime.UtcNow, "user1");

        var taskId1 = new OnboardingTaskId(Guid.NewGuid());
        var task1 = OnboardingTask.Create(
            taskId1, "Task 1", null,
            AssigneeRoleCode.It, "user2", "John Doe",
            "user1", DateTime.UtcNow.AddDays(7), 1);
        instance.AddTask(task1);

        var taskId2 = new OnboardingTaskId(Guid.NewGuid());
        var task2 = OnboardingTask.Create(
            taskId2, "Task 2", null,
            AssigneeRoleCode.Hr, "user3", "Jane Doe",
            "user1", DateTime.UtcNow.AddDays(14), 2);
        instance.AddTask(task2);

        instance.CompleteTask(taskId1, "user2");

        _instanceRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingInstance, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(instance);

        var command = new CompleteOnboardingTaskCommand(taskId1, null)
        {
            CompletedBy = "user3"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.HrOnboardingTaskAlreadyCompleted);
    }
}
