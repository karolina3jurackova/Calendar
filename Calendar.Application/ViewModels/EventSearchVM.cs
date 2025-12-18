using System.ComponentModel.DataAnnotations;

namespace Calendar.Application.ViewModels;

public class EventSearchQueryVM
{
    public string? Q { get; set; }
    public string? Text { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? From { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? To { get; set; }
}