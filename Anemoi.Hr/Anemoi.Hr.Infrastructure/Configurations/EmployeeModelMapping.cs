using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class EmployeeModelMapping : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeId(id));

        builder.Property(x => x.PrimaryDepartmentId)
            .HasConversion(x => x.Value, id => new DepartmentId(id));
        builder.Property(x => x.PrimaryPositionId)
            .HasConversion(x => x.Value, id => new PositionId(id));
        builder.Property(x => x.DirectManagerEmployeeId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, 
                id => id == null ? null : new EmployeeId(id.Value));

        builder.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200);
        builder.Property(x => x.AvatarStorageKey).HasMaxLength(500);
        builder.Ignore(x => x.FullName); // computed property
        builder.Property(x => x.WorkEmail).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PersonalEmail).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.EmploymentStatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EmploymentTypeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.GradeCode).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.EmployeeCode).IsUnique().HasDatabaseName("ix_employees_employee_code");

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        // No explicit relationship (HasForeignKey/WithMany) configuration here.
        // Navigation properties (PrimaryDepartment, PrimaryPosition, DirectManager,
        // DirectReports) are resolved by EF conventions. This avoids circular dependency
        // issues during seed data batch saves where Department ↔ Employee reference each other.
    }
}
