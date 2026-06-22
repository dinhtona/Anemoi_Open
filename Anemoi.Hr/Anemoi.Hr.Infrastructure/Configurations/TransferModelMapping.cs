using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Transfers;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class TransferModelMapping : IEntityTypeConfiguration<EmployeeTransfer>
{
    public void Configure(EntityTypeBuilder<EmployeeTransfer> builder)
    {
        builder.ToTable("EmployeeTransfers", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeTransferId(id));

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.SourceDepartmentId)
            .HasConversion(x => x.Value, id => new DepartmentId(id));
        builder.Property(x => x.TargetDepartmentId)
            .HasConversion(x => x.Value, id => new DepartmentId(id));
        builder.Property(x => x.SourcePositionId)
            .HasConversion(x => x.Value, id => new PositionId(id));
        builder.Property(x => x.TargetPositionId)
            .HasConversion(x => x.Value, id => new PositionId(id));
        builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ReviewedBy).HasMaxLength(200);
        builder.Property(x => x.ReviewComment).HasMaxLength(500);
        builder.Property(x => x.WorkflowInstanceId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new WorkflowInstanceId(id.Value));

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SourceDepartment)
            .WithMany()
            .HasForeignKey(x => x.SourceDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetDepartment)
            .WithMany()
            .HasForeignKey(x => x.TargetDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourcePosition)
            .WithMany()
            .HasForeignKey(x => x.SourcePositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetPosition)
            .WithMany()
            .HasForeignKey(x => x.TargetPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.StatusCode });
        builder.HasIndex(x => x.EffectiveDate);
    }
}
