using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Contracts;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Infrastructure.Persistence;

public sealed class HrDbContext(DbContextOptions<HrDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeIdentityLinkLog> EmployeeIdentityLinkLogs { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<EmployeeDepartmentHistory> EmployeeDepartmentHistories { get; set; }
    public DbSet<EmployeePositionHistory> EmployeePositionHistories { get; set; }
    public DbSet<EmployeeGradeHistory> EmployeeGradeHistories { get; set; }
    public DbSet<EmployeeManagerHistory> EmployeeManagerHistories { get; set; }
    public DbSet<LeavePolicy> LeavePolicies { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveTransaction> LeaveTransactions { get; set; }
    public DbSet<LeaveAccrualRun> LeaveAccrualRuns { get; set; }
    public DbSet<EmployeeContract> EmployeeContracts { get; set; }
    public DbSet<SalaryGrade> SalaryGrades { get; set; }
    public DbSet<SalaryRange> SalaryRanges { get; set; }
    public DbSet<EmployeeSalary> EmployeeSalaries { get; set; }
    public DbSet<AllowanceType> AllowanceTypes { get; set; }
    public DbSet<PositionAllowance> PositionAllowances { get; set; }
    public DbSet<EmployeeAllowance> EmployeeAllowances { get; set; }
    public DbSet<SalaryValidationBypassLog> SalaryValidationBypassLogs { get; set; }
    public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
    public DbSet<PayrollRun> PayrollRuns { get; set; }
    public DbSet<PayrollItem> PayrollItems { get; set; }
    public DbSet<AttendancePeriod> AttendancePeriods { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IHrInfrastructureAssemblyMarker).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
