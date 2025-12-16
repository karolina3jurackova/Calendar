using Calendar.Application.Abstraction.Services;
using Calendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class RemindersController : ControllerBase
{
    private readonly IEventService _service;
    private readonly UserManager<ApplicationUser> _userManager;

    public RemindersController(
        IEventService service,
        UserManager<ApplicationUser> userManager)
    {
        _service = service;
        _userManager = userManager;
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            throw new InvalidOperationException("User not authenticated");

        return user.Id;
    }

    // GET /api/my-reminders
    [HttpGet("my-reminders")]
    public async Task<IActionResult> MyRemindersApi()
    {
        var userId = await GetCurrentUserIdAsync();
        var items = await _service.GetMyRemindersAsync(userId);
        return Ok(items);
    }
}