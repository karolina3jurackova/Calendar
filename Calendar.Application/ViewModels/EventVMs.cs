using System.ComponentModel.DataAnnotations;

namespace Calendar.Application.ViewModels;

public class EventListItemVM
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

