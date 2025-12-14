using System.ComponentModel.DataAnnotations;

namespace CalendarMvcApp.Models.Admin.Users;

public class AdminUserEditVM
{
    public Guid Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    // checkboxy vo View
    public bool IsAdmin { get; set; }
    public bool IsManager { get; set; }
    public bool IsCustomer { get; set; }
}