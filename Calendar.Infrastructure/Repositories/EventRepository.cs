using Calendar.Application.Abstraction.Repositories;
using Calendar.Domain.Entities;
using Calendar.Infrastructure.Data;          // <- toto!
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db) => _db = db;

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<IList<Event>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => _db.Events.Where(e => e.OwnerId == userId).ToListAsync(ct)
            .ContinueWith(t => (IList<Event>)t.Result, ct);

    public Task AddAsync(Event entity, CancellationToken ct = default)
        => _db.Events.AddAsync(entity, ct).AsTask();

    public void Update(Event entity) => _db.Events.Update(entity);
    public void Remove(Event entity) => _db.Events.Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}