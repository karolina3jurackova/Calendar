using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configurations;

public sealed class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> b)
    {
        b.ToTable("Reminders");

        b.HasKey(x => x.Id);

        b.Property(x => x.Type)
            .IsRequired();

        b.Property(x => x.Channel)
            .IsRequired();

        // Relative / Absolute sú voliteľné (podľa Type)
        b.Property(x => x.MinutesBefore)
            .IsRequired(false);

        b.Property(x => x.AbsoluteUtc)
            .IsRequired(false);

        // výsledný čas kedy sa má pripomenúť (UTC)
        b.Property(x => x.FireAtUtc)
            .IsRequired();

        b.Property(x => x.IsSent)
            .HasDefaultValue(false)
            .IsRequired();

        b.Property(x => x.CreatedUtc)
            .IsRequired();

        // Event (1) -> Reminders (0..N)
        b.HasOne(x => x.Event)
            .WithMany(e => e.Reminders)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // indexy pre scheduler / query
        b.HasIndex(x => new { x.EventId, x.FireAtUtc });
        b.HasIndex(x => new { x.IsSent, x.FireAtUtc });

        b.Property(x => x.RepeatEveryMinutes).IsRequired(false);
        b.Property(x => x.RepeatCountLeft).IsRequired(false);
    }
}