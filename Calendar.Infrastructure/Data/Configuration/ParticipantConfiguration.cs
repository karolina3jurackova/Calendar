using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configuration;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Event)
         .WithMany(e => e.Participants)
         .HasForeignKey(x => x.EventId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.EventId, x.UserId, x.Email }).IsUnique();

        b.Property(x => x.Email).HasMaxLength(254);
        b.Property(x => x.Nickname).HasMaxLength(120);
    }
}