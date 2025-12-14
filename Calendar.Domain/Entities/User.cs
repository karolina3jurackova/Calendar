namespace Calendar.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Nickname { get; set; } = default!;
    public DateTime DateCreated { get; set; }
    public DateTime LastModified { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}