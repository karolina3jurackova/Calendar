using Calendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Controllers;

// USERS API - pomocné API pre autocomplete / vyhľadanie usera podľa emailu
[Authorize]
[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager; // prístup k Users query (Identity)

    public UsersApiController(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    // GET /api/users/search?email=kar@
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string email)
    {
        email = (email ?? "").Trim();
        if (email.Length < 2) return Ok(Array.Empty<object>()); // ochrana proti spam/ťažkým query

        var term = email.ToLower();

        var res = _userManager.Users
            .Where(u => u.Email != null && u.Email.ToLower().Contains(term)) // filtrovanie podľa emailu
            .OrderBy(u => u.Email)
            .Take(10) // limit výsledkov
            .Select(u => new { id = u.Id, email = u.Email }) // vraciame len minimum dát (bez citlivých vecí)
            .ToList();

        return Ok(res);
    }
}