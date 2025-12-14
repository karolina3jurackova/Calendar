using System.ComponentModel.DataAnnotations;
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

    // ======================
    // LIST
    // ======================
    public async Task<IActionResult> Index()
    {
        var userId = await GetCurrentUserIdAsync();
        var vm = await _service.GetMyEventsAsync(userId);
        return View(vm);
    }

    // ======================
    // DETAILS
    // ======================
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var vm = await _service.GetDetailAsync(id, userId);
        if (vm == null) return NotFound();
        return View(vm);
    }

    // ======================
    // CREATE (GET)  ✅ napojené na Home kalendár
    // /Events/Create?date=YYYY-MM-DD
    // ======================
    [HttpGet]
    public IActionResult Create(string? date)
    {
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var d))
        {
            var start = d.ToDateTime(new TimeOnly(9, 0));
            var end = d.ToDateTime(new TimeOnly(10, 0));

            return View(new EventCreateVM
            {
                Start = start,
                End = end
            });
        }

        // default keď prídeš ručne
        return View(new EventCreateVM
        {
            Start = DateTime.Now.AddMinutes(15),
            End = DateTime.Now.AddMinutes(45)
        });
    }

    // CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventCreateVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var userId = await GetCurrentUserIdAsync();
            var id = await _service.CreateAsync(userId, vm);
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(vm);
        }
    }

    // ======================
    // API pre Home kalendár
    // ======================
    [HttpGet]
    [Route("api/my-events")]
    public async Task<IActionResult> MyEventsApi()
    {
        var userId = await GetCurrentUserIdAsync();
        var items = await _service.GetMyCalendarEventsAsync(userId);
        return Ok(items);
    }

    // ======================
    // EDIT (GET)
    // ======================
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var detail = await _service.GetDetailAsync(id, userId);
        if (detail == null) return NotFound();

        var vm = new EventEditVM
        {
            Title = detail.Title,
            Description = detail.Description,
            Start = detail.Start,
            End = detail.End
        };

        ViewBag.EventId = id;
        return View(vm);
    }

    // EDIT (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EventEditVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventId = id;
            return View(vm);
        }

        try
        {
            var userId = await GetCurrentUserIdAsync();
            var ok = await _service.UpdateAsync(id, userId, vm);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.EventId = id;
            return View(vm);
        }
    }

    // ======================
    // DELETE (GET confirm)
    // ======================
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var vm = await _service.GetDetailAsync(id, userId);
        if (vm == null) return NotFound();
        return View(vm);
    }

    // DELETE (POST)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var userId = await GetCurrentUserIdAsync();
        var ok = await _service.DeleteAsync(id, userId);
        if (!ok) return NotFound();

        return RedirectToAction(nameof(Index));
    }
}