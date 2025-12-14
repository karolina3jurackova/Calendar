using Calendar.Application.ViewModels;

namespace Calendar.Application.Abstraction.Services;

public interface IEventService
{
    Task<IList<EventListItemVM>> GetMyEventsAsync(Guid userId, CancellationToken ct = default);
    Task<EventDetailVM?> GetDetailAsync(Guid id, Guid userId, CancellationToken ct = default);

    Task<Guid> CreateAsync(Guid userId, EventCreateVM vm, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, Guid userId, EventEditVM vm, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}