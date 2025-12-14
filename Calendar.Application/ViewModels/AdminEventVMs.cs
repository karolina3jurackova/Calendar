using System.ComponentModel.DataAnnotations;

namespace Calendar.Application.ViewModels;

public sealed class AdminEventListItemVM
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
    public Guid OwnerId { get; set; }
}

public sealed class AdminEventEditVM
{
    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime Start { get; set; }  // local (UI)

    [Required]
    public DateTime End { get; set; }    // local (UI)

    public Guid OwnerId { get; set; } // admin môže meniť owner (voliteľne)
}