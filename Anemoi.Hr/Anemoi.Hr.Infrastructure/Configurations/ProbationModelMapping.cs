using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Hr.Infrastructure.Configurations;

public sealed class ProbationModelMapping : IEntityTypeConfiguration<ProbationRecord>
{
    public void Configure(EntityTypeBuilder<ProbationRecord> builder)
    {
        builder.ToTable("ProbationRecords", "Hr");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new ProbationRecordId(id));

        builder.Property(x => x.EmployeeId)
            .HasConversion(x => x.Value, id => new EmployeeId(id));
        builder.Property(x => x.ReviewerEmployeeId)
            .HasConversion(x => x == null ? default(Guid?) : x.Value, id => id == null ? null : new EmployeeId(id.Value));
        builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Result).HasMaxLength(50);
        builder.Property(x => x.Comment).HasMaxLength(500);

        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReviewerEmployee)
            .WithMany()
            .HasForeignKey(x => x.ReviewerEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.EmployeeId, x.StartDate });
        builder.HasIndex(x => x.StatusCode);
    }
}
