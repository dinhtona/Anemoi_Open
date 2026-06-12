using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.ModelIds.ModelIds;
using Xunit;

namespace Anemoi.BuildingBlock.Test.HrOvertimeTests;

public class OvertimeRequestDomainTests
{
    private static readonly EmployeeId TestEmployeeId = new(Guid.NewGuid());

    [Fact]
    public void Create_SetsPendingStatus()
    {
        var request = CreateValidRequest();
        Assert.Equal(OvertimeStatusCode.Pending, request.Status);
    }

    [Fact]
    public void CalculateDurationHours_SameDay_ReturnsCorrectHours()
    {
        var request = CreateRequest(TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"));
        Assert.Equal(2m, request.CalculateDurationHours());
    }

    [Fact]
    public void CalculateDurationHours_ZeroMinutes_ReturnsWholeNumber()
    {
        var request = CreateRequest(TimeOnly.Parse("09:00"), TimeOnly.Parse("17:00"));
        Assert.Equal(8m, request.CalculateDurationHours());
    }

    [Fact]
    public void CalculateDurationHours_WithMinutes_ReturnsDecimal()
    {
        var request = CreateRequest(TimeOnly.Parse("18:00"), TimeOnly.Parse("20:30"));
        Assert.Equal(2.5m, request.CalculateDurationHours());
    }

    [Fact]
    public void CalculateDurationHours_MidnightToEarlyMorning_SameDay()
    {
        var request = CreateRequest(TimeOnly.Parse("23:00"), TimeOnly.Parse("23:30"));
        Assert.Equal(0.5m, request.CalculateDurationHours());
    }

    [Fact]
    public void Approve_FromPending_SetsApproved()
    {
        var request = CreateValidRequest();
        request.Approve("manager1");
        Assert.Equal(OvertimeStatusCode.Approved, request.Status);
        Assert.Equal("manager1", request.ApprovedBy);
        Assert.NotNull(request.ApprovedAt);
    }

    [Fact]
    public void Approve_FromApproved_Throws()
    {
        var request = CreateValidRequest();
        request.Approve("manager1");
        Assert.Throws<InvalidOperationException>(() => request.Approve("manager2"));
    }

    [Fact]
    public void Approve_FromRejected_Throws()
    {
        var request = CreateValidRequest();
        request.Reject("manager1");
        Assert.Throws<InvalidOperationException>(() => request.Approve("manager2"));
    }

    [Fact]
    public void Approve_FromCancelled_Throws()
    {
        var request = CreateValidRequest();
        request.Cancel("employee1");
        Assert.Throws<InvalidOperationException>(() => request.Approve("manager1"));
    }

    [Fact]
    public void Reject_FromPending_SetsRejected()
    {
        var request = CreateValidRequest();
        request.Reject("manager1");
        Assert.Equal(OvertimeStatusCode.Rejected, request.Status);
        Assert.Equal("manager1", request.RejectedBy);
        Assert.NotNull(request.RejectedAt);
    }

    [Fact]
    public void Reject_FromApproved_Throws()
    {
        var request = CreateValidRequest();
        request.Approve("manager1");
        Assert.Throws<InvalidOperationException>(() => request.Reject("manager2"));
    }

    [Fact]
    public void Reject_FromRejected_Throws()
    {
        var request = CreateValidRequest();
        request.Reject("manager1");
        Assert.Throws<InvalidOperationException>(() => request.Reject("manager2"));
    }

    [Fact]
    public void Reject_FromCancelled_Throws()
    {
        var request = CreateValidRequest();
        request.Cancel("employee1");
        Assert.Throws<InvalidOperationException>(() => request.Reject("manager1"));
    }

    [Fact]
    public void Cancel_FromPending_SetsCancelled()
    {
        var request = CreateValidRequest();
        request.Cancel("employee1");
        Assert.Equal(OvertimeStatusCode.Cancelled, request.Status);
        Assert.Equal("employee1", request.CancelledBy);
        Assert.NotNull(request.CancelledAt);
    }

    [Fact]
    public void Cancel_FromApproved_Throws()
    {
        var request = CreateValidRequest();
        request.Approve("manager1");
        Assert.Throws<InvalidOperationException>(() => request.Cancel("employee1"));
    }

    [Fact]
    public void Cancel_FromRejected_Throws()
    {
        var request = CreateValidRequest();
        request.Reject("manager1");
        Assert.Throws<InvalidOperationException>(() => request.Cancel("employee1"));
    }

    [Fact]
    public void Cancel_FromCancelled_Throws()
    {
        var request = CreateValidRequest();
        request.Cancel("employee1");
        Assert.Throws<InvalidOperationException>(() => request.Cancel("employee2"));
    }

    [Fact]
    public void OverlapsWith_SameTime_ReturnsTrue()
    {
        var a = CreateRequest(TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"));
        var b = CreateRequest(TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"));
        Assert.True(a.OverlapsWith(b));
    }

    [Fact]
    public void OverlapsWith_PartialOverlap_ReturnsTrue()
    {
        var a = CreateRequest(TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"));
        var b = CreateRequest(TimeOnly.Parse("19:00"), TimeOnly.Parse("21:00"));
        Assert.True(a.OverlapsWith(b));
    }

    [Fact]
    public void OverlapsWith_Contained_ReturnsTrue()
    {
        var a = CreateRequest(TimeOnly.Parse("18:00"), TimeOnly.Parse("22:00"));
        var b = CreateRequest(TimeOnly.Parse("19:00"), TimeOnly.Parse("20:00"));
        Assert.True(a.OverlapsWith(b));
    }

    [Fact]
    public void OverlapsWith_Adjacent_ReturnsFalse()
    {
        var a = CreateRequest(TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"));
        var b = CreateRequest(TimeOnly.Parse("20:00"), TimeOnly.Parse("22:00"));
        Assert.False(a.OverlapsWith(b));
    }

    [Fact]
    public void OverlapsWith_DifferentDates_ReturnsFalse()
    {
        var a = CreateRequest(TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"));
        var b = CreateOvertimeRequest(
            TestEmployeeId,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TimeOnly.Parse("18:00"),
            TimeOnly.Parse("20:00"),
            "Test");
        Assert.False(a.OverlapsWith(b));
    }

    [Fact]
    public void OverlapsWith_NonOverlapping_ReturnsFalse()
    {
        var a = CreateRequest(TimeOnly.Parse("08:00"), TimeOnly.Parse("10:00"));
        var b = CreateRequest(TimeOnly.Parse("11:00"), TimeOnly.Parse("13:00"));
        Assert.False(a.OverlapsWith(b));
    }

    [Fact]
    public void Create_NullEmployeeId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            OvertimeRequest.Create(null!, DateOnly.FromDateTime(DateTime.Today),
                TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"), "Test"));
    }

    [Fact]
    public void Create_StartAfterEnd_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateOvertimeRequest(TestEmployeeId, DateOnly.FromDateTime(DateTime.Today),
                TimeOnly.Parse("20:00"), TimeOnly.Parse("18:00"), "Test"));
    }

    [Fact]
    public void Create_StartEqualsEnd_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateOvertimeRequest(TestEmployeeId, DateOnly.FromDateTime(DateTime.Today),
                TimeOnly.Parse("18:00"), TimeOnly.Parse("18:00"), "Test"));
    }

    [Fact]
    public void Create_EmptyReason_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateOvertimeRequest(TestEmployeeId, DateOnly.FromDateTime(DateTime.Today),
                TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"), ""));
    }

    private static OvertimeRequest CreateValidRequest()
    {
        return CreateOvertimeRequest(TestEmployeeId, DateOnly.FromDateTime(DateTime.Today),
            TimeOnly.Parse("18:00"), TimeOnly.Parse("20:00"), "Overtime work");
    }

    private static OvertimeRequest CreateRequest(TimeOnly start, TimeOnly end)
    {
        return CreateOvertimeRequest(TestEmployeeId, DateOnly.FromDateTime(DateTime.Today),
            start, end, "Test");
    }

    private static OvertimeRequest CreateOvertimeRequest(
        EmployeeId employeeId,
        DateOnly overtimeDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string reason)
    {
        return OvertimeRequest.Create(employeeId, overtimeDate, startTime, endTime, reason);
    }
}
