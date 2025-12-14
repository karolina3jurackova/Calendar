using System.ComponentModel.DataAnnotations;

namespace CalendarMvcApp.Models.Account;

public class LoginVM
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}