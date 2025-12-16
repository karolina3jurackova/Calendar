using Calendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersApiController(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    // GET /api/users/search?email=kar@
    [HttpGet("search")]
    public IActionResult Search([FromQuery] string email)
    {
        email = (email ?? "").Trim();
        if (email.Length < 2) return Ok(Array.Empty<object>());

        var term = email.ToLower();

        var res = _userManager.Users
            .Where(u => u.Email != null && u.Email.ToLower().Contains(term))
            .OrderBy(u => u.Email)
            .Take(10)
            .Select(u => new { id = u.Id, email = u.Email })
            .ToList();

        return Ok(res);
    }
}