using System.Collections.Generic;
using System.Text.Json;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Contract.Notification.ModelIds;
using Anemoi.Notification.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Anemoi.Notification.Infrastructure.DataContext;

public sealed class ModelMapping :
    IEntityTypeConfiguration<NotificationHistory>,
    IEntityTypeConfiguration<NotificationSubscription>,
    IEntityTypeConfiguration<NotificationPreference>,
    IEntityTypeConfiguration<NotificationAction>,
    IEntityTypeConfiguration<NotificationActionAudit>
{
    public void Configure(EntityTypeBuilder<NotificationHistory> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new NotificationHistoryId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId)
            .HasConversion(x => x.Value, id => new UserId(id))
            .IsRequired();
        
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.WorkspaceId);
        builder.HasIndex(x => x.CreatedTime);
        builder.HasIndex(x => new { x.UserId, x.IsHidden });
        builder.HasIndex(x => new { x.UserId, x.CreatedTime });
        builder.HasIndex(x => new { x.UserId, x.IsArchived });

        builder.Property(x => x.TitleLocalizationArgs)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
            .HasColumnType("text");

        builder.Property(x => x.ContentLocalizationArgs)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
            .HasColumnType("text");

        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => new { x.UserId, x.DeduplicationKey })
            .IsUnique()
            .HasFilter("\"DeduplicationKey\" IS NOT NULL");

        builder.Navigation(x => x.Actions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Actions)
            .WithOne(x => x.Notification)
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<NotificationSubscription> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new NotificationSubscriptionId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId)
            .HasConversion(x => x.Value, id => new UserId(id))
            .IsRequired();
        
        builder.HasIndex(x => new { x.UserId, x.Category }).IsUnique();
    }

    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new NotificationPreferenceId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId)
            .HasConversion(x => x.Value, id => new UserId(id))
            .IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.EnableInApp).HasDefaultValue(true);
        builder.Property(x => x.EnableEmail).HasDefaultValue(true);
    }

    public void Configure(EntityTypeBuilder<NotificationAction> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new NotificationActionId(id));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NotificationId)
            .HasConversion(x => x.Value, id => new NotificationHistoryId(id));

        builder.Property(x => x.ActionCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ActionLabel).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ActionUrl).HasMaxLength(500);
        builder.Property(x => x.ActionType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.RequiresConfirmation).HasDefaultValue(false);
        builder.Property(x => x.SortOrder).HasDefaultValue(0);

        builder.HasIndex(x => x.NotificationId);
    }

    public void Configure(EntityTypeBuilder<NotificationActionAudit> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new NotificationActionAuditId(id));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExecutedBy)
            .HasConversion(x => x.Value, id => new UserId(id))
            .IsRequired();

        builder.Property(x => x.NotificationId)
            .HasConversion(x => x.Value, id => new NotificationHistoryId(id));

        builder.Property(x => x.ActionId)
            .HasConversion(x => x.Value, id => new NotificationActionId(id));

        builder.Property(x => x.Result).HasMaxLength(1000);
        builder.Property(x => x.ClientIp).HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasMaxLength(500);

        builder.HasIndex(x => x.NotificationId);
        builder.HasIndex(x => x.ExecutedAt);
        builder.HasIndex(x => x.ExecutedBy);
    }
}
