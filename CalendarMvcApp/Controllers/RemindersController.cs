using Calendar.Application.Abstraction.Services;
using Calendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Controllers;

// REMINDERS API - endpointy pre site.js (modal/notifications)
[Authorize]
[ApiController]
[Route("api")]
public class RemindersController : ControllerBase
{
    private readonly IEventService _service;                 // business logika reminderov
    private readonly UserManager<ApplicationUser> _userManager; // aktuálny user

    public RemindersController(IEventService service, UserManager<ApplicationUser> userManager)
    {
        _service = service;
        _userManager = userManager;
    }

    // helper: aktuálny userId
    private async Task<Guid> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            throw new InvalidOperationException("User not authenticated");

        return user.Id;
    }

    // GET /api/my-reminders - vráti reminders pre prihláseného usera
    [HttpGet("my-reminders")]
    public async Task<IActionResult> MyRemindersApi()
    {
        var userId = await GetCurrentUserIdAsync();
        var items = await _service.GetMyRemindersAsync(userId);
        return Ok(items);
    }

    // POST /api/my-reminders/{id}/seen - označí reminder ako zobrazený (IsSent=true)
    [HttpPost("my-reminders/{id}/seen")]
    public async Task<IActionResult> MarkAsSeen(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var ok = await _service.MarkReminderAsSentAsync(id, userId);

        if (!ok)
            return NotFound();

        return NoContent();
    }
}