using Calendar.Domain.Entities;

namespace Calendar.Application.Abstraction.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IList<Event>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    Task AddAsync(Event entity, CancellationToken ct = default);
    void Update(Event entity);
    void Remove(Event entity);

    Task SaveChangesAsync(CancellationToken ct = default);
}