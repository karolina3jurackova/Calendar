using Calendar.Application.Abstraction.Repositories;
using Calendar.Domain.Entities;
using Calendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;
    public EventRepository(AppDbContext db) => _db = db;

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Events
            .Include(e => e.EventShares)   // ✅ dôležité pre sharing/permissions
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IList<Event>> GetForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Events
            .AsNoTracking()
            .Include(e => e.EventShares)   // ✅ nech vieme zobraziť / overiť shares aj bez ďalšieho dopytu
            .Where(e =>
                e.OwnerId == userId ||
                e.EventShares.Any(s => s.SharedWithUserId == userId)
            )
            .OrderBy(e => e.StartTime)
            .ToListAsync(ct);
    }

    public async Task<IList<Event>> GetAllAsync(CancellationToken ct = default)
        => await _db.Events.ToListAsync(ct);

    public Task AddAsync(Event entity, CancellationToken ct = default)
        => _db.Events.AddAsync(entity, ct).AsTask();

    public void Update(Event entity) => _db.Events.Update(entity);
    public void Remove(Event entity) => _db.Events.Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    // =========================
    // SHARES
    // =========================
    public async Task ReplaceEventSharesAsync(Guid eventId, IList<Guid> sharedWithUserIds, CancellationToken ct = default)
    {
        // zmaž existujúce user-shares pre event
        var existing = await _db.EventShares
            .Where(x => x.EventId == eventId && x.SharedWithUserId != null)
            .ToListAsync(ct);

        _db.EventShares.RemoveRange(existing);

        foreach (var uid in sharedWithUserIds.Distinct())
        {
            _db.EventShares.Add(new EventShare
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                SharedWithUserId = uid,
                Access = EventShareAccess.Read,
                CreatedUtc = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    // =========================
    // REMINDERS
    // =========================
    public async Task<IList<Reminder>> GetRemindersForUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Reminders
            .Include(r => r.Event)
            .Where(r => r.Event.OwnerId == userId)
            .ToListAsync(ct);

    public Task AddReminderAsync(Reminder reminder, CancellationToken ct = default)
        => _db.Reminders.AddAsync(reminder, ct).AsTask();

    public async Task<Reminder?> GetReminderByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Reminders
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public void RemoveReminder(Reminder reminder)
        => _db.Reminders.Remove(reminder);
}