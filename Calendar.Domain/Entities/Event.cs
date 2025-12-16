using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Calendar.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }

    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool AllDay { get; set; } = false;
    public string? Location { get; set; }

    public string? RepeatRule { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastModified { get; set; }

    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public ICollection<Share> Shares { get; set; } = new List<Share>();

    public ICollection<EventShare> EventShares { get; set; } = new List<EventShare>();

    [NotMapped] public List<DateTime> SkipDates { get; set; } = new();
}