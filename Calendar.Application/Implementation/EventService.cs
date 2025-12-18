using Calendar.Application.Abstraction.Repositories;
using Calendar.Application.Abstraction.Services;
using Calendar.Application.ViewModels;
using Calendar.Domain.Entities;

namespace Calendar.Application.Implementation;

public class EventService : IEventService
{
    private readonly IEventRepository _repo;
    private readonly IUserLookupRepository _userLookup;

    public EventService(IEventRepository repo, IUserLookupRepository userLookup)
    {
        _repo = repo;
        _userLookup = userLookup;
    }

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

        // owner + shared
        var myEvents = await _repo.GetForUserAsync(userId, ct);
        var e = myEvents.FirstOrDefault(x => x.Id == id);
        if (e == null) return null;

        // Reminders ukazujeme iba ownerovi 
        var forEvent = new List<ReminderItemVM>();
        if (e.OwnerId == userId)
        {
            var reminders = await _repo.GetRemindersForUserAsync(userId, ct);
            forEvent = reminders
                .Where(r => r.EventId == e.Id)
                .OrderBy(r => r.FireAtUtc)
                .Select(r => new ReminderItemVM
                {
                    Id = r.Id,
                    Type = r.Type,
                    Channel = r.Channel,
                    MinutesBefore = r.MinutesBefore,
                    FireAtLocal = FromUtcToLocal(r.FireAtUtc),
                    IsSent = r.IsSent
                })
                .ToList();
        }

        return new EventDetailVM
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Start = FromUtcToLocal(e.StartTime),
            End = FromUtcToLocal(e.EndTime),
            Location = e.Location,
            AllDay = e.AllDay,
            Reminders = forEvent
        };
    }

    // =========================
    // CREATE / UPDATE / DELETE
    // =========================

    public async Task<Guid> CreateAsync(Guid userId, EventCreateVM vm, CancellationToken ct = default)
    {
        EnsureValidUser(userId);
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

        // sharing podľa emailu
        var sharedUserIds = await ResolveUserIdsFromEmailsAsync(vm.ShareWithEmails, userId, ct);
        await _repo.ReplaceEventSharesAsync(entity.Id, sharedUserIds, ct);

        if (HasExplicitTime(vm.Start, vm.End))
        {
            var autoReminder = new Reminder
            {
                Id = Guid.NewGuid(),
                EventId = entity.Id,
                Type = ReminderType.AbsoluteUtc,
                Channel = ReminderChannel.Browser,
                MinutesBefore = null,
                AbsoluteUtc = entity.StartTime,
                FireAtUtc = entity.StartTime,
                IsSent = false,
                CreatedUtc = DateTime.UtcNow
            };

            await _repo.AddReminderAsync(autoReminder, ct);
            await _repo.SaveChangesAsync(ct);
        }

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

        var sharedUserIds = await ResolveUserIdsFromEmailsAsync(vm.ShareWithEmails, userId, ct);
        await _repo.ReplaceEventSharesAsync(entity.Id, sharedUserIds, ct);

        // AUTO-REMINDER
        var myReminders = await _repo.GetRemindersForUserAsync(userId, ct);
        var toDelete = myReminders
            .Where(r =>
                r.EventId == entity.Id &&
                r.Channel == ReminderChannel.Browser &&
                r.Type == ReminderType.AbsoluteUtc)
            .ToList();

        foreach (var r in toDelete)
            _repo.RemoveReminder(r);

        await _repo.SaveChangesAsync(ct);

        if (HasExplicitTime(vm.Start, vm.End))
        {
            var newAuto = new Reminder
            {
                Id = Guid.NewGuid(),
                EventId = entity.Id,
                Type = ReminderType.AbsoluteUtc,
                Channel = ReminderChannel.Browser,
                MinutesBefore = null,
                AbsoluteUtc = entity.StartTime,
                FireAtUtc = entity.StartTime,
                IsSent = false,
                CreatedUtc = DateTime.UtcNow
            };

            await _repo.AddReminderAsync(newAuto, ct);
            await _repo.SaveChangesAsync(ct);
        }

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

    public async Task<IList<EventCalendarVM>> GetMyCalendarEventsAsync(Guid userId, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var events = await _repo.GetForUserAsync(userId, ct);

        // reminders len ownerove 
        var reminders = await _repo.GetRemindersForUserAsync(userId, ct);

        var remByEvent = reminders
            .GroupBy(r => r.EventId)
            .ToDictionary(g => g.Key, g => g.Select(r => new ReminderItemVM
            {
                Id = r.Id,
                Type = r.Type,
                Channel = r.Channel,
                MinutesBefore = r.MinutesBefore,
                FireAtLocal = FromUtcToLocal(r.FireAtUtc),
                IsSent = r.IsSent
            }).OrderBy(x => x.FireAtLocal).ToList());

        return events.Select(e => new EventCalendarVM
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Start = FromUtcToLocal(e.StartTime),
            End = FromUtcToLocal(e.EndTime),
            Reminders = remByEvent.TryGetValue(e.Id, out var list) ? list : new List<ReminderItemVM>()
        }).ToList();
    }

    // =========================
    // SEARCH
    // =========================

    public async Task<IList<EventListItemVM>> SearchMyEventsAsync(Guid userId, EventSearchQueryVM q, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var events = await _repo.GetForUserAsync(userId, ct);
        IEnumerable<Event> query = events;

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var term = q.Q.Trim().ToLowerInvariant();
            query = query.Where(e =>
                (e.Title ?? "").ToLowerInvariant().Contains(term) ||
                (e.Description ?? "").ToLowerInvariant().Contains(term)
            );
        }

        if (q.From.HasValue)
            query = query.Where(e => FromUtcToLocal(e.StartTime) >= q.From.Value);

        if (q.To.HasValue)
            query = query.Where(e => FromUtcToLocal(e.EndTime) <= q.To.Value);

        return query
            .OrderBy(e => e.StartTime)
            .Select(e => new EventListItemVM
            {
                Id = e.Id,
                Title = e.Title,
                Start = FromUtcToLocal(e.StartTime),
                End = FromUtcToLocal(e.EndTime)
            })
            .ToList();
    }

    // =========================
    // REMINDERS (API pre site.js)
    // =========================

    public async Task<IList<ReminderNotifyVM>> GetMyRemindersAsync(Guid userId, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var reminders = await _repo.GetRemindersForUserAsync(userId, ct);
        var nowUtc = DateTime.UtcNow;

        // (okno -10s až +20s, aby to fungovalo aj s pollingom každých 10s)
        var fromUtc = nowUtc.AddSeconds(-10);
        var toUtc = nowUtc.AddSeconds(20);

        return reminders
            .Where(r =>
                !r.IsSent &&
                r.Channel == ReminderChannel.Browser &&
                r.FireAtUtc >= fromUtc &&
                r.FireAtUtc <= toUtc
            )
            .OrderBy(r => r.FireAtUtc)
            .Select(r => new ReminderNotifyVM
            {
                Id = r.Id,
                FireAtUtc = r.FireAtUtc,
                EventTitle = r.Event.Title,
                Message =
                    r.Type == ReminderType.RelativeMinutesBefore
                        ? $"Pripomienka: {r.MinutesBefore} min pred udalosťou"
                        : "Pripomienka udalosti"
            })
            .ToList();
    }

    public async Task<bool> AddReminderAsync(Guid eventId, Guid userId, ReminderCreateVM vm, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var ev = await _repo.GetByIdAsync(eventId, ct);
        if (ev == null) return false;
        if (ev.OwnerId != userId) return false;

        DateTime fireAtUtc;

        if (vm.Type == ReminderType.RelativeMinutesBefore)
        {
            if (vm.MinutesBefore is null) throw new ArgumentException("MinutesBefore is required.");
            fireAtUtc = ev.StartTime.AddMinutes(-vm.MinutesBefore.Value);
        }
        else
        {
            if (vm.AbsoluteLocal is null) throw new ArgumentException("AbsoluteLocal is required.");
            var local = DateTime.SpecifyKind(vm.AbsoluteLocal.Value, DateTimeKind.Local);
            fireAtUtc = local.ToUniversalTime();
        }

        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            EventId = ev.Id,
            Type = vm.Type,
            Channel = vm.Channel,
            MinutesBefore = vm.MinutesBefore,
            AbsoluteUtc = vm.Type == ReminderType.AbsoluteUtc ? fireAtUtc : null,
            FireAtUtc = fireAtUtc,
            IsSent = false,
            CreatedUtc = DateTime.UtcNow,
            RepeatEveryMinutes = vm.RepeatEveryMinutes,
            RepeatCountLeft = vm.RepeatCountLeft
        };

        await _repo.AddReminderAsync(reminder, ct);
        await _repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteReminderAsync(Guid reminderId, Guid userId, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var r = await _repo.GetReminderByIdAsync(reminderId, ct);
        if (r == null) return false;
        if (r.Event.OwnerId != userId) return false;

        _repo.RemoveReminder(r);
        await _repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkReminderAsSentAsync(Guid reminderId, Guid userId, CancellationToken ct = default)
    {
        EnsureValidUser(userId);

        var reminder = await _repo.GetReminderByIdAsync(reminderId, ct);
        if (reminder == null) return false;

        if (reminder.Event.OwnerId != userId) return false;
        if (reminder.IsSent) return true;

        reminder.IsSent = true;
        await _repo.SaveChangesAsync(ct);

        return true;
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
    // SHARING HELPERS
    // =========================

    private async Task<IList<Guid>> ResolveUserIdsFromEmailsAsync(string? emails, Guid ownerId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(emails))
            return new List<Guid>();

        var emailList = emails
            .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => x.Contains("@"))
            .Distinct()
            .ToList();

        if (emailList.Count == 0)
            return new List<Guid>();

        var ids = await _userLookup.GetUserIdsByEmailsAsync(emailList, ct);

        return ids
            .Where(x => x != ownerId)
            .Distinct()
            .ToList();
    }

    // =========================
    // HELPERS
    // =========================

    private static bool HasExplicitTime(DateTime startLocal, DateTime endLocal)
    {
        // Ak user nevyplní čas, typicky skončíš na 00:00.
        return startLocal.TimeOfDay != TimeSpan.Zero || endLocal.TimeOfDay != TimeSpan.Zero;
    }

    private static void EnsureValidUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user id.");
    }

    private static void ValidateDatesOrThrow(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("End must be after Start.");
    }

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