using Calendar.Application.Abstraction.Repositories;
using Calendar.Application.Abstraction.Services;
using Calendar.Application.ViewModels;
using Calendar.Domain.Entities;

namespace Calendar.Application.Implementation;

public class EventService : IEventService
{
    private readonly IEventRepository _repo;

    public EventService(IEventRepository repo) => _repo = repo;

    public async Task<IList<EventListItemVM>> GetMyEventsAsync(Guid userId, CancellationToken ct = default)
    {
        var events = await _repo.GetForUserAsync(userId, ct);

        return events.Select(e => new EventListItemVM
        {
            Id = e.Id,
            Title = e.Title,
            Start = e.StartTime,
            End = e.EndTime
        }).ToList();
    }

    public async Task<EventDetailVM?> GetDetailAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(id, ct);
        if (e == null) return null;

        // ak chceš základnú autorizáciu:
        // if (e.OwnerId != userId) return null;

        return new EventDetailVM
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Start = e.StartTime,
            End = e.EndTime
        };
    }

    public async Task<Guid> CreateAsync(Guid userId, EventCreateVM vm, CancellationToken ct = default)
    {
        if (vm.End <= vm.Start)
            throw new ArgumentException("End must be after Start.");

        var entity = new Event
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            Title = vm.Title.Trim(),
            Description = vm.Description?.Trim(),
            StartTime = vm.Start,
            EndTime = vm.End,
            DateCreated = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, EventEditVM vm, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return false;

        if (vm.End <= vm.Start)
            throw new ArgumentException("End must be after Start.");

        // if (entity.OwnerId != userId) return false;

        entity.Title = vm.Title.Trim();
        entity.Description = vm.Description?.Trim();
        entity.StartTime = vm.Start;
        entity.EndTime = vm.End;
        entity.LastModified = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return false;

        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}