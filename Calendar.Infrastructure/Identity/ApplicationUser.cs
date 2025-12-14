using Microsoft.AspNetCore.Identity;

namespace Calendar.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    // sem si môžeš doplniť vlastné polia (napr. FirstName, LastName)
}
