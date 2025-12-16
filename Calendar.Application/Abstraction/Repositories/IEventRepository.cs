using Calendar.Domain.Entities;

namespace Calendar.Application.Abstraction.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IList<Event>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    // ADMIN
    Task<IList<Event>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Event entity, CancellationToken ct = default);
    void Update(Event entity);
    void Remove(Event entity);

    Task<IList<Reminder>> GetRemindersForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddReminderAsync(Reminder reminder, CancellationToken ct = default);
    Task<Reminder?> GetReminderByIdAsync(Guid id, CancellationToken ct = default);
    void RemoveReminder(Reminder reminder);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task ReplaceEventSharesAsync(Guid eventId, IList<Guid> sharedWithUserIds, CancellationToken ct = default);
}