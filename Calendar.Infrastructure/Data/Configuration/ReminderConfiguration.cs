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

        // Typ pripomienky (relatívna / absolútna)
        b.Property(x => x.Type)
            .IsRequired();

        // Kanál notifikácie (Browser, Email)
        b.Property(x => x.Channel)
            .IsRequired();

        // Počet minút pred udalosťou
        b.Property(x => x.MinutesBefore)
            .IsRequired(false);

        // Absolútny UTC čas 
        b.Property(x => x.AbsoluteUtc)
            .IsRequired(false);

        // (frontend + backend už pracujú len s týmto časom)
        b.Property(x => x.FireAtUtc)
            .IsRequired();

        b.Property(x => x.IsSent)
            .HasDefaultValue(false)
            .IsRequired();

        b.Property(x => x.CreatedUtc)
            .IsRequired();

        // Pri zmazaní eventu sa zmažú aj jeho pripomienky
        b.HasOne(x => x.Event)
            .WithMany(e => e.Reminders)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexy pre rýchle vyhľadávanie reminderov
        b.HasIndex(x => new { x.EventId, x.FireAtUtc });
        b.HasIndex(x => new { x.IsSent, x.FireAtUtc });

        b.Property(x => x.RepeatEveryMinutes)
            .IsRequired(false);

        b.Property(x => x.RepeatCountLeft)
            .IsRequired(false);
    }
}