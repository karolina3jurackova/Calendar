using Calendar.Application.Abstraction.Services;
using Calendar.Application.ViewModels;
using Calendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Controllers;

[Authorize]
public class EventsController : Controller
{
    private readonly IEventService _service;
    private readonly UserManager<ApplicationUser> _userManager;

    public EventsController(IEventService service, UserManager<ApplicationUser> userManager)
    {
        _service = service;
        _userManager = userManager;
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) throw new InvalidOperationException("User is not authenticated.");
        return user.Id;
    }

    public async Task<IActionResult> Index()
    {
        var userId = await GetCurrentUserIdAsync();
        var vm = await _service.GetMyEventsAsync(userId);
        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var vm = await _service.GetDetailAsync(id, userId);
        if (vm == null) return NotFound();
        return View(vm);
    }

    public IActionResult Create() => View(new EventCreateVM());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventCreateVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var userId = await GetCurrentUserIdAsync();
        var id = await _service.CreateAsync(userId, vm);

        return RedirectToAction(nameof(Details), new { id });
    }
}