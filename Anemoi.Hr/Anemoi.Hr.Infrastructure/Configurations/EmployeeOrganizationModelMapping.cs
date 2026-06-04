using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class EmployeeOrganizationModelMapping :
    IEntityTypeConfiguration<Employee>,
    IEntityTypeConfiguration<Department>,
    IEntityTypeConfiguration<Position>,
    IEntityTypeConfiguration<EmployeeDepartmentHistory>,
    IEntityTypeConfiguration<EmployeePositionHistory>,
    IEntityTypeConfiguration<EmployeeGradeHistory>,
    IEntityTypeConfiguration<EmployeeManagerHistory>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.PrimaryDepartmentId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new DepartmentId(id.Value));
        builder.Property(x => x.PrimaryPositionId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new PositionId(id.Value));
        builder.Property(x => x.DirectManagerEmployeeId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new EmployeeId(id.Value));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.WorkEmail).HasMaxLength(256);
        builder.Property(x => x.PersonalEmail).HasMaxLength(256);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.EmploymentStatusCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EmploymentTypeCode).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.EmployeeCode).IsUnique();
        builder.HasIndex(x => x.WorkEmail).IsUnique();
        builder.HasOne(x => x.PrimaryDepartment)
            .WithMany(x => x.PrimaryEmployees)
            .HasForeignKey(x => x.PrimaryDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.PrimaryPosition)
            .WithMany(x => x.PrimaryEmployees)
            .HasForeignKey(x => x.PrimaryPositionId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.DirectManager)
            .WithMany(x => x.DirectReports)
            .HasForeignKey(x => x.DirectManagerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new DepartmentId(id));
        builder.Property(x => x.ParentDepartmentId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new DepartmentId(id.Value));
        builder.Property(x => x.ManagerEmployeeId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new EmployeeId(id.Value));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.DepartmentTypeCode).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasOne(x => x.ParentDepartment)
            .WithMany(x => x.ChildDepartments)
            .HasForeignKey(x => x.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ManagerEmployee)
            .WithMany()
            .HasForeignKey(x => x.ManagerEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new PositionId(id));
        builder.Property(x => x.DepartmentId)
            .HasConversion(x => x.Value, id => new DepartmentId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PositionTypeCode).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasOne(x => x.Department)
            .WithMany(x => x.Positions)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EmployeeDepartmentHistory> builder)
    {
        builder.ToTable("EmployeeDepartmentHistories");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeDepartmentHistoryId(id));
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.DepartmentId)
            .HasConversion(x => x.Value, id => new DepartmentId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReasonCode).HasMaxLength(64);
        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom });
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.DepartmentHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EmployeePositionHistory> builder)
    {
        builder.ToTable("EmployeePositionHistories");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeePositionHistoryId(id));
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.PositionId)
            .HasConversion(x => x.Value, id => new PositionId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReasonCode).HasMaxLength(64);
        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom });
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.PositionHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EmployeeGradeHistory> builder)
    {
        builder.ToTable("EmployeeGradeHistories");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeGradeHistoryId(id));
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GradeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ReasonCode).HasMaxLength(64);
        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom });
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.GradeHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<EmployeeManagerHistory> builder)
    {
        builder.ToTable("EmployeeManagerHistories");
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeManagerHistoryId(id));
        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.ManagerEmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReasonCode).HasMaxLength(64);
        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom });
        builder.HasOne(x => x.Employee)
            .WithMany(x => x.ManagerHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ManagerEmployee)
            .WithMany()
            .HasForeignKey(x => x.ManagerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
