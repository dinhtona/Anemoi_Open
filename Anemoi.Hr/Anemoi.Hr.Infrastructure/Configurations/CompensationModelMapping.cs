using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class SalaryGradeModelMapping : IEntityTypeConfiguration<SalaryGrade>
{
    public void Configure(EntityTypeBuilder<SalaryGrade> builder)
    {
        builder.ToTable("SalaryGrades");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new SalaryGradeId(id));
        builder.Property(x => x.GradeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        
        builder.HasIndex(x => x.GradeCode).IsUnique();
        
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class SalaryRangeModelMapping : IEntityTypeConfiguration<SalaryRange>
{
    public void Configure(EntityTypeBuilder<SalaryRange> builder)
    {
        builder.ToTable("SalaryRanges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new SalaryRangeId(id));
        builder.Property(x => x.SalaryGradeId).HasConversion(x => x.Value, id => new SalaryGradeId(id));
        builder.Property(x => x.Currency).HasMaxLength(16).IsRequired();
        
        builder.HasOne(x => x.SalaryGrade)
            .WithMany(x => x.Ranges)
            .HasForeignKey(x => x.SalaryGradeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class EmployeeSalaryModelMapping : IEntityTypeConfiguration<EmployeeSalary>
{
    public void Configure(EntityTypeBuilder<EmployeeSalary> builder)
    {
        builder.ToTable("EmployeeSalaries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new EmployeeSalaryId(id));
        builder.Property(x => x.EmployeeId).HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.SalaryGradeId).HasConversion(x => x.Value, id => new SalaryGradeId(id));
        builder.Property(x => x.GradeCodeSnapshot).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(16).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SalaryGrade)
            .WithMany()
            .HasForeignKey(x => x.SalaryGradeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom })
            .IsUnique();

        builder.HasIndex(x => x.EmployeeId)
            .IsUnique()
            .HasFilter("\"EffectiveTo\" IS NULL");

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class AllowanceTypeModelMapping : IEntityTypeConfiguration<AllowanceType>
{
    public void Configure(EntityTypeBuilder<AllowanceType> builder)
    {
        builder.ToTable("AllowanceTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new AllowanceTypeId(id));
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.Taxable).IsRequired();
        builder.Property(x => x.SubjectToInsurance).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class PositionAllowanceModelMapping : IEntityTypeConfiguration<PositionAllowance>
{
    public void Configure(EntityTypeBuilder<PositionAllowance> builder)
    {
        builder.ToTable("PositionAllowances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new PositionAllowanceId(id));
        builder.Property(x => x.PositionId).HasConversion(x => x.Value, id => new PositionId(id));
        builder.Property(x => x.AllowanceTypeId).HasConversion(x => x.Value, id => new AllowanceTypeId(id));
        builder.Property(x => x.Currency).HasMaxLength(16).IsRequired();

        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AllowanceType)
            .WithMany()
            .HasForeignKey(x => x.AllowanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.PositionId, x.AllowanceTypeId }).IsUnique();

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class EmployeeAllowanceModelMapping : IEntityTypeConfiguration<EmployeeAllowance>
{
    public void Configure(EntityTypeBuilder<EmployeeAllowance> builder)
    {
        builder.ToTable("EmployeeAllowances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new EmployeeAllowanceId(id));
        builder.Property(x => x.EmployeeId).HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.AllowanceTypeId).HasConversion(x => x.Value, id => new AllowanceTypeId(id));
        builder.Property(x => x.Currency).HasMaxLength(16).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128);
        builder.Property(x => x.UpdatedBy).HasMaxLength(128);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AllowanceType)
            .WithMany()
            .HasForeignKey(x => x.AllowanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.AllowanceTypeId, x.Currency, x.EffectiveFrom })
            .IsUnique();

        builder.HasIndex(x => new { x.EmployeeId, x.AllowanceTypeId, x.Currency })
            .IsUnique()
            .HasFilter("\"EffectiveTo\" IS NULL");

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public sealed class SalaryValidationBypassLogModelMapping : IEntityTypeConfiguration<SalaryValidationBypassLog>
{
    public void Configure(EntityTypeBuilder<SalaryValidationBypassLog> builder)
    {
        builder.ToTable("SalaryValidationBypassLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, id => new SalaryValidationBypassLogId(id));
        builder.Property(x => x.EmployeeId).HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.GradeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(16).IsRequired();
        builder.Property(x => x.BypassReason).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128);

        builder.HasIndex(x => x.EmployeeId);

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
