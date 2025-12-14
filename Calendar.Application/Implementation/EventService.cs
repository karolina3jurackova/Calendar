using Calendar.Application.Abstraction.Repositories;
using Calendar.Application.Abstraction.Services;
using Calendar.Application.ViewModels;
using Calendar.Domain.Entities;

namespace Calendar.Application.Implementation;

public class EventService : IEventService
{
    private readonly IEventRepository _repo;

    public EventService(IEventRepository repo) => _repo = repo;

    // =========================
    // USER
    // =========================

    public async Task<IList<EventListItemVM>> GetMyEventsAsync(Guid userId, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var events = await _repo.GetForUserAsync(userId, ct);

        return events.Select(e => new EventListItemVM
        {
            Id = e.Id,
            Title = e.Title,
            Start = FromUtcToLocal(e.StartTime),
            End = FromUtcToLocal(e.EndTime)
        }).ToList();
    }

    public async Task<EventDetailVM?> GetDetailAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var e = await _repo.GetByIdAsync(id, ct);
        if (e == null) return null;
        if (e.OwnerId != userId) return null;

        return new EventDetailVM
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Start = FromUtcToLocal(e.StartTime),
            End = FromUtcToLocal(e.EndTime)
        };
    }

    public async Task<Guid> CreateAsync(Guid userId, EventCreateVM vm, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        // ✅ Service nerieši "minulosť" ani UI validáciu – to má byť vo VM (DataAnnotations + custom attribute)
        ValidateDatesOrThrow(vm.Start, vm.End);

        var nowUtc = DateTime.UtcNow;

        var entity = new Event
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            Title = vm.Title.Trim(),
            Description = vm.Description?.Trim(),
            StartTime = ToUtc(vm.Start),
            EndTime = ToUtc(vm.End),
            DateCreated = nowUtc,
            LastModified = nowUtc
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, EventEditVM vm, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        ValidateDatesOrThrow(vm.Start, vm.End);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return false;
        if (entity.OwnerId != userId) return false;

        entity.Title = vm.Title.Trim();
        entity.Description = vm.Description?.Trim();
        entity.StartTime = ToUtc(vm.Start);
        entity.EndTime = ToUtc(vm.End);
        entity.LastModified = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return false;
        if (entity.OwnerId != userId) return false;

        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);

        return true;
    }

    // ✅ JSON feed pre Home kalendár (site.js)
    public async Task<IList<EventCalendarVM>> GetMyCalendarEventsAsync(Guid userId, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var events = await _repo.GetForUserAsync(userId, ct);

        return events.Select(e => new EventCalendarVM
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Start = FromUtcToLocal(e.StartTime),
            End = FromUtcToLocal(e.EndTime)
        }).ToList();
    }

    // =========================
    // ADMIN
    // =========================

    public async Task<IList<AdminEventListItemVM>> AdminGetAllAsync(CancellationToken ct = default)
    {
        var events = await _repo.GetAllAsync(ct);

        return events
            .OrderByDescending(e => e.StartTime)
            .Select(e => new AdminEventListItemVM
            {
                Id = e.Id,
                Title = e.Title,
                StartLocal = FromUtcToLocal(e.StartTime),
                EndLocal = FromUtcToLocal(e.EndTime),
                OwnerId = e.OwnerId
            })
            .ToList();
    }

    public async Task<AdminEventEditVM?> AdminGetEditAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(id, ct);
        if (e == null) return null;

        return new AdminEventEditVM
        {
            Title = e.Title,
            Description = e.Description,
            Start = FromUtcToLocal(e.StartTime),
            End = FromUtcToLocal(e.EndTime),
            OwnerId = e.OwnerId
        };
    }

    public async Task<bool> AdminUpdateAsync(Guid id, AdminEventEditVM vm, CancellationToken ct = default)
    {
        // Admin validácia: title + end>start
        if (string.IsNullOrWhiteSpace(vm.Title)) return false;
        if (vm.End <= vm.Start) return false;

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return false;

        entity.Title = vm.Title.Trim();
        entity.Description = vm.Description?.Trim();
        entity.StartTime = ToUtc(vm.Start);
        entity.EndTime = ToUtc(vm.End);
        entity.OwnerId = vm.OwnerId;
        entity.LastModified = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AdminDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return false;

        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);
        return true;
    }

    // =========================
    // HELPERS
    // =========================

    private static void EnsureValidUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user id.");
    }

    // Táto validácia je "core" (koniec musí byť po začiatku).
    // Minulosť rieš custom attribute na VM (kvôli splneniu kritéria a UX).
    private static void ValidateDatesOrThrow(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("End must be after Start.");
    }

    // datetime-local = lokálny čas bez timezone -> berieme ako Local a konvertujeme do UTC
    private static DateTime ToUtc(DateTime localDateTime)
    {
        var local = DateTime.SpecifyKind(localDateTime, DateTimeKind.Local);
        return local.ToUniversalTime();
    }

    private static DateTime FromUtcToLocal(DateTime utcDateTime)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return utc.ToLocalTime();
    }
}