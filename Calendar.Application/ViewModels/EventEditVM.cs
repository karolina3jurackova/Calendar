using System;
using System.ComponentModel.DataAnnotations;
using Calendar.Application.Validation;

namespace Calendar.Application.ViewModels;

public class EventEditVM
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    [Required]
    [NotPastDate] // ✅
    public DateTime Start { get; set; }

    [Required]
    public DateTime End { get; set; }
}