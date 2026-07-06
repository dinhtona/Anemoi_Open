using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Separations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class SeparationModelMapping : IEntityTypeConfiguration<EmployeeSeparation>
{
    public void Configure(EntityTypeBuilder<EmployeeSeparation> builder)
    {
        builder.ToTable("EmployeeSeparations", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new EmployeeSeparationId(id));

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.SeparationTypeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Details).HasMaxLength(2000);
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

        builder.HasIndex(x => new { x.EmployeeId, x.StatusCode });
        builder.HasIndex(x => x.SeparationDate);
        builder.HasIndex(x => x.LastWorkingDate);
    }
}
