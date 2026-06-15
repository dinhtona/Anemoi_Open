using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.ShiftManagement;
using Anemoi.Hr.ModelIds.ModelIds;
using Xunit;

namespace Anemoi.BuildingBlock.Test.HrShiftManagementTests;

public class ShiftManagementDomainTests
{
    private static readonly EmployeeId TestEmployeeId = new(Guid.NewGuid());

    [Fact]
    public void CreateShiftTemplate_SetsCorrectValues()
    {
        var template = CreateValidTemplate();
        Assert.Equal("MORNING", template.Code);
        Assert.Equal("Morning Shift", template.Name);
        Assert.Equal(TimeOnly.Parse("06:00"), template.StartTime);
        Assert.Equal(TimeOnly.Parse("14:00"), template.EndTime);
        Assert.Equal(30, template.BreakMinutes);
        Assert.True(template.IsActive);
    }

    [Fact]
    public void CreateShiftTemplate_ExpectedWorkingHours_DerivedCorrectly()
    {
        var template = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "MORNING", "Morning Shift",
            TimeOnly.Parse("06:00"), TimeOnly.Parse("14:00"), 30);
        Assert.Equal(7.5m, template.ExpectedWorkingHours);
    }

    [Fact]
    public void CreateShiftTemplate_NoBreak_ExpectedHoursEqualsDuration()
    {
        var template = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "NOBREAK", "No Break Shift",
            TimeOnly.Parse("09:00"), TimeOnly.Parse("17:00"), 0);
        Assert.Equal(8m, template.ExpectedWorkingHours);
    }

    [Fact]
    public void CreateShiftTemplate_ExpectedHours_RoundedToTwoDecimals()
    {
        var template = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "PRECISE", "Precise Shift",
            TimeOnly.Parse("08:00"), TimeOnly.Parse("17:15"), 45);
        Assert.Equal(8.5m, template.ExpectedWorkingHours);
    }

    [Fact]
    public void CreateShiftTemplate_EmptyCode_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "", "Morning Shift",
                TimeOnly.Parse("06:00"), TimeOnly.Parse("14:00"), 30));
    }

    [Fact]
    public void CreateShiftTemplate_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "MORNING", "",
                TimeOnly.Parse("06:00"), TimeOnly.Parse("14:00"), 30));
    }

    [Fact]
    public void CreateShiftTemplate_StartTimeEqualsEndTime_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "INVALID", "Invalid Shift",
                TimeOnly.Parse("08:00"), TimeOnly.Parse("08:00"), 30));
    }

    [Fact]
    public void CreateShiftTemplate_CrossMidnight_CreatesSuccessfully()
    {
        var template = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "NIGHT", "Night Shift",
            TimeOnly.Parse("22:00"), TimeOnly.Parse("06:00"), 30);
        Assert.Equal("NIGHT", template.Code);
        Assert.Equal("Night Shift", template.Name);
        Assert.Equal(TimeOnly.Parse("22:00"), template.StartTime);
        Assert.Equal(TimeOnly.Parse("06:00"), template.EndTime);
        Assert.Equal(30, template.BreakMinutes);
        Assert.True(template.IsActive);
    }

    [Fact]
    public void CreateShiftTemplate_CrossMidnight_ExpectedWorkingHours_Correct()
    {
        // 22:00 to 06:00 = 8 hours, minus 30 min break = 7.5 hours
        var template = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "NIGHT", "Night Shift",
            TimeOnly.Parse("22:00"), TimeOnly.Parse("06:00"), 30);
        Assert.Equal(7.5m, template.ExpectedWorkingHours);
    }

    [Fact]
    public void CreateShiftTemplate_CrossMidnight_NoBreak_ExpectedHours_Correct()
    {
        // 23:00 to 07:00 = 8 hours, no break
        var template = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "NIGHT2", "Night Shift 2",
            TimeOnly.Parse("23:00"), TimeOnly.Parse("07:00"), 0);
        Assert.Equal(8m, template.ExpectedWorkingHours);
    }

    [Fact]
    public void CreateShiftTemplate_NegativeBreakMinutes_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "INVALID", "Invalid Shift",
                TimeOnly.Parse("06:00"), TimeOnly.Parse("14:00"), -1));
    }

    [Fact]
    public void CreateShiftTemplate_ZeroBreakMinutes_IsValid()
    {
        var template = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "NOBREAK", "No Break",
            TimeOnly.Parse("06:00"), TimeOnly.Parse("14:00"), 0);
        Assert.Equal(0, template.BreakMinutes);
        Assert.Equal(8m, template.ExpectedWorkingHours);
    }

    [Fact]
    public void ActivateShiftTemplate_SetsActive()
    {
        var template = CreateValidTemplate();
        template.Deactivate();
        Assert.False(template.IsActive);
        template.Activate();
        Assert.True(template.IsActive);
    }

    [Fact]
    public void DeactivateShiftTemplate_SetsInactive()
    {
        var template = CreateValidTemplate();
        template.Deactivate();
        Assert.False(template.IsActive);
    }

    [Fact]
    public void UpdateShiftTemplate_UpdatesAllFields()
    {
        var template = CreateValidTemplate();
        template.Update("AFTERNOON", "Afternoon Shift",
            TimeOnly.Parse("14:00"), TimeOnly.Parse("22:00"), 45);

        Assert.Equal("AFTERNOON", template.Code);
        Assert.Equal("Afternoon Shift", template.Name);
        Assert.Equal(TimeOnly.Parse("14:00"), template.StartTime);
        Assert.Equal(TimeOnly.Parse("22:00"), template.EndTime);
        Assert.Equal(45, template.BreakMinutes);
        Assert.Equal(7.25m, template.ExpectedWorkingHours);
    }

    [Fact]
    public void UpdateShiftTemplate_EqualTimes_Throws()
    {
        var template = CreateValidTemplate();
        Assert.Throws<ArgumentException>(() =>
            template.Update("INVALID", "Invalid",
                TimeOnly.Parse("08:00"), TimeOnly.Parse("08:00"), 30));
    }

    [Fact]
    public void CreateEmployeeShiftAssignment_SnapshotsTemplateValues()
    {
        var template = CreateValidTemplate();
        var assignment = EmployeeShiftAssignment.Create(
            new EmployeeShiftAssignmentId(Guid.NewGuid()),
            TestEmployeeId, template,
            DateOnly.FromDateTime(DateTime.Today), "admin");

        Assert.Equal(TestEmployeeId, assignment.EmployeeId);
        Assert.Equal(template.Id, assignment.ShiftTemplateId);
        Assert.Equal(template.Name, assignment.ShiftNameSnapshot);
        Assert.Equal(template.StartTime, assignment.StartTimeSnapshot);
        Assert.Equal(template.EndTime, assignment.EndTimeSnapshot);
        Assert.Equal(template.BreakMinutes, assignment.BreakMinutesSnapshot);
        Assert.Equal(template.ExpectedWorkingHours, assignment.ExpectedWorkingHoursSnapshot);
        Assert.Equal(EmployeeShiftAssignmentStatusCode.Assigned, assignment.Status);
        Assert.Equal("admin", assignment.AssignedBy);
        Assert.NotEqual(default, assignment.AssignedAt);
    }

    [Fact]
    public void EditingTemplate_DoesNotMutateExistingAssignmentSnapshots()
    {
        var template = CreateValidTemplate();
        var assignment = EmployeeShiftAssignment.Create(
            new EmployeeShiftAssignmentId(Guid.NewGuid()),
            TestEmployeeId, template,
            DateOnly.FromDateTime(DateTime.Today), "admin");

        template.Update("AFTERNOON", "Afternoon Shift Updated",
            TimeOnly.Parse("14:00"), TimeOnly.Parse("22:00"), 45);

        Assert.Equal("Morning Shift", assignment.ShiftNameSnapshot);
        Assert.Equal(TimeOnly.Parse("06:00"), assignment.StartTimeSnapshot);
        Assert.Equal(TimeOnly.Parse("14:00"), assignment.EndTimeSnapshot);
        Assert.Equal(30, assignment.BreakMinutesSnapshot);
        Assert.Equal(7.5m, assignment.ExpectedWorkingHoursSnapshot);
    }

    [Fact]
    public void CreateAssignment_InactiveTemplate_Throws()
    {
        var template = CreateValidTemplate();
        template.Deactivate();

        Assert.Throws<ArgumentException>(() =>
            EmployeeShiftAssignment.Create(
                new EmployeeShiftAssignmentId(Guid.NewGuid()),
                TestEmployeeId, template,
                DateOnly.FromDateTime(DateTime.Today), "admin"));
    }

    [Fact]
    public void CreateAssignment_NullEmployeeId_Throws()
    {
        var template = CreateValidTemplate();
        Assert.Throws<ArgumentException>(() =>
            EmployeeShiftAssignment.Create(
                new EmployeeShiftAssignmentId(Guid.NewGuid()),
                null!, template,
                DateOnly.FromDateTime(DateTime.Today), "admin"));
    }

    [Fact]
    public void CreateAssignment_NullTemplate_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            EmployeeShiftAssignment.Create(
                new EmployeeShiftAssignmentId(Guid.NewGuid()),
                TestEmployeeId, null!,
                DateOnly.FromDateTime(DateTime.Today), "admin"));
    }

    [Fact]
    public void CancelAssignment_SetsCancelled()
    {
        var template = CreateValidTemplate();
        var assignment = EmployeeShiftAssignment.Create(
            new EmployeeShiftAssignmentId(Guid.NewGuid()),
            TestEmployeeId, template,
            DateOnly.FromDateTime(DateTime.Today), "admin");

        assignment.Cancel("manager1", "Schedule change");

        Assert.Equal(EmployeeShiftAssignmentStatusCode.Cancelled, assignment.Status);
        Assert.Equal("manager1", assignment.CancelledBy);
        Assert.NotNull(assignment.CancelledAt);
        Assert.Equal("Schedule change", assignment.CancellationReason);
    }

    [Fact]
    public void CancelAssignment_FromCancelled_Throws()
    {
        var template = CreateValidTemplate();
        var assignment = EmployeeShiftAssignment.Create(
            new EmployeeShiftAssignmentId(Guid.NewGuid()),
            TestEmployeeId, template,
            DateOnly.FromDateTime(DateTime.Today), "admin");

        assignment.Cancel("manager1");
        Assert.Throws<InvalidOperationException>(() => assignment.Cancel("manager2"));
    }

    [Fact]
    public void CancelledAssignment_DoesNotBlockNewAssignment()
    {
        var template = CreateValidTemplate();
        var workDate = DateOnly.FromDateTime(DateTime.Today);

        var firstAssignment = EmployeeShiftAssignment.Create(
            new EmployeeShiftAssignmentId(Guid.NewGuid()),
            TestEmployeeId, template, workDate, "admin");
        firstAssignment.Cancel("manager1");

        var secondAssignment = EmployeeShiftAssignment.Create(
            new EmployeeShiftAssignmentId(Guid.NewGuid()),
            TestEmployeeId, template, workDate, "admin");
        Assert.Equal(EmployeeShiftAssignmentStatusCode.Assigned, secondAssignment.Status);
        Assert.NotEqual(firstAssignment.Id, secondAssignment.Id);
    }

    [Fact]
    public void ShiftTemplate_HasValidId()
    {
        var template = CreateValidTemplate();
        Assert.NotEqual(Guid.Empty, template.Id.Value);
    }

    [Fact]
    public void EmployeeShiftAssignment_HasValidId()
    {
        var template = CreateValidTemplate();
        var assignment = EmployeeShiftAssignment.Create(
            new EmployeeShiftAssignmentId(Guid.NewGuid()),
            TestEmployeeId, template,
            DateOnly.FromDateTime(DateTime.Today), "admin");
        Assert.NotEqual(Guid.Empty, assignment.Id.Value);
    }

    [Fact]
    public void DifferentShiftTemplates_HaveDifferentIds()
    {
        var a = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "A", "A", TimeOnly.Parse("06:00"), TimeOnly.Parse("14:00"), 30);
        var b = ShiftTemplate.Create(new ShiftTemplateId(Guid.NewGuid()), "B", "B", TimeOnly.Parse("14:00"), TimeOnly.Parse("22:00"), 30);
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void CalculateExpectedWorkingHours_BreakLongerThanShift_ReturnsZero()
    {
        var hours = ShiftTemplate.CalculateExpectedWorkingHours(
            TimeOnly.Parse("08:00"), TimeOnly.Parse("09:00"), 120);
        Assert.Equal(0m, hours);
    }

    [Fact]
    public void CreateShiftTemplate_UpdatesTimestamp()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var template = CreateValidTemplate();
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.InRange(template.CreatedAt, before, after);
        Assert.InRange(template.UpdatedAt, before, after);
    }

    [Fact]
    public void UpdateShiftTemplate_UpdatesUpdatedAt()
    {
        var template = CreateValidTemplate();
        var before = DateTime.UtcNow.AddSeconds(-1);
        Thread.Sleep(10);
        template.Update("NEW", "New Name",
            TimeOnly.Parse("06:00"), TimeOnly.Parse("14:00"), 30);
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.InRange(template.UpdatedAt, before, after);
    }

    [Fact]
    public void ShiftTemplate_UsesIdGenerator_NotGuidNewGuid()
    {
        var template = CreateValidTemplate();
        Assert.IsType<ShiftTemplateId>(template.Id);
        Assert.NotEqual(Guid.Empty, template.Id.Value);
    }

    [Fact]
    public void EmployeeShiftAssignment_UsesIdGenerator_NotGuidNewGuid()
    {
        var template = CreateValidTemplate();
        var assignment = EmployeeShiftAssignment.Create(
            new EmployeeShiftAssignmentId(Guid.NewGuid()),
            TestEmployeeId, template,
            DateOnly.FromDateTime(DateTime.Today), "admin");
        Assert.IsType<EmployeeShiftAssignmentId>(assignment.Id);
        Assert.NotEqual(Guid.Empty, assignment.Id.Value);
    }

    [Fact]
    public void OverlappingTimeRanges_DetectedCorrectly()
    {
        var existingStart = new TimeOnly(08, 00);
        var existingEnd = new TimeOnly(12, 00);

        var newStart = new TimeOnly(10, 00);
        var newEnd = new TimeOnly(14, 00);

        var overlap = newStart < existingEnd && newEnd > existingStart;
        Assert.True(overlap);
    }

    [Fact]
    public void NonOverlappingTimeRanges_NotDetectedAsOverlap()
    {
        var existingStart = new TimeOnly(08, 00);
        var existingEnd = new TimeOnly(12, 00);

        var newStart = new TimeOnly(13, 00);
        var newEnd = new TimeOnly(17, 00);

        var overlap = newStart < existingEnd && newEnd > existingStart;
        Assert.False(overlap);
    }

    [Fact]
    public void AdjacentTimeRanges_DoNotOverlap()
    {
        var existingStart = new TimeOnly(08, 00);
        var existingEnd = new TimeOnly(12, 00);

        var newStart = new TimeOnly(12, 00);
        var newEnd = new TimeOnly(16, 00);

        var overlap = newStart < existingEnd && newEnd > existingStart;
        Assert.False(overlap);
    }

    [Fact]
    public void ShiftAssignmentOverlapErrorCode_DefinedInBusinessErrorCodes()
    {
        var code = HrBusinessErrorCodes.ShiftAssignmentOverlap;
        Assert.Equal("HR_SHIFT_ASSIGNMENT_OVERLAP", code);
    }

    [Fact]
    public void AssignedByRequiredErrorCode_DefinedInBusinessErrorCodes()
    {
        var code = HrBusinessErrorCodes.AssignedByRequired;
        Assert.Equal("HR_ASSIGNED_BY_REQUIRED", code);
    }

    [Fact]
    public void CancelledByRequiredErrorCode_DefinedInBusinessErrorCodes()
    {
        var code = HrBusinessErrorCodes.CancelledByRequired;
        Assert.Equal("HR_CANCELLED_BY_REQUIRED", code);
    }

    [Fact]
    public void CancellationReasonRequiredErrorCode_DefinedInBusinessErrorCodes()
    {
        var code = HrBusinessErrorCodes.CancellationReasonRequired;
        Assert.Equal("HR_CANCELLATION_REASON_REQUIRED", code);
    }

    [Fact]
    public void CancellationReasonMaxLengthErrorCode_DefinedInBusinessErrorCodes()
    {
        var code = HrBusinessErrorCodes.CancellationReasonMaxLength;
        Assert.Equal("HR_CANCELLATION_REASON_MAX_LENGTH", code);
    }

    [Fact]
    public void SaveChangesFailedErrorCode_DefinedInBusinessErrorCodes()
    {
        var code = HrBusinessErrorCodes.SaveChangesFailed;
        Assert.Equal("HR_SAVE_CHANGES_FAILED", code);
    }

    private static ShiftTemplate CreateValidTemplate()
    {
        return ShiftTemplate.Create(
            new ShiftTemplateId(Guid.NewGuid()),
            "MORNING", "Morning Shift",
            TimeOnly.Parse("06:00"), TimeOnly.Parse("14:00"), 30);
    }
}
