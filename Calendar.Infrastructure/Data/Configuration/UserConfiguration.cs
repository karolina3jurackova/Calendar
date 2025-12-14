using Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Calendar.Infrastructure.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Email).IsRequired().HasMaxLength(254);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
        b.Property(x => x.Nickname).IsRequired().HasMaxLength(50);

        b.Property(x => x.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(x => x.LastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}