using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Calendar.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }

    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    // TIME 
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool AllDay { get; set; } = false;
    public string? Location { get; set; }

    // AUDIT
    public DateTime DateCreated { get; set; }
    public DateTime LastModified { get; set; }

    // browser reminders
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    // zdieľanie eventov
    public ICollection<EventShare> EventShares { get; set; } = new List<EventShare>();


    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public ICollection<Share> Shares { get; set; } = new List<Share>();

}