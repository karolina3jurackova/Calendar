namespace CalendarMvcApp.Models.Admin.Users;

public class AdminUserListItemVM
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public IList<string> Roles { get; set; } = new List<string>();
}