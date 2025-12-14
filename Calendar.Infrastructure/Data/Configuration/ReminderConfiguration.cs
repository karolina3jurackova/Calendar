using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configuration;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Event)
         .WithMany(e => e.Reminders)
         .HasForeignKey(x => x.EventId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.EventId);
    }
}