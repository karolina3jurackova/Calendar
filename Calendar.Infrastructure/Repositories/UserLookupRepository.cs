using Calendar.Application.Abstraction.Repositories;
using Calendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public sealed class UserLookupRepository : IUserLookupRepository
{
    private readonly AppDbContext _db;
    public UserLookupRepository(AppDbContext db) => _db = db;

    public async Task<IList<Guid>> GetUserIdsByEmailsAsync(IList<string> emails, CancellationToken ct = default)
    {
        if (emails.Count == 0) return new List<Guid>();

        var normalized = emails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        // ApplicationUser je v Identity tabuľkách, AppDbContext ich má cez IdentityDbContext
        return await _db.Users
            .Where(u => u.Email != null && normalized.Contains(u.Email.ToLower()))
            .Select(u => u.Id)
            .ToListAsync(ct);
    }
}