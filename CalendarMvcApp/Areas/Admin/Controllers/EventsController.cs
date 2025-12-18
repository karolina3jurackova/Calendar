using Calendar.Application.Abstraction.Services;
using Calendar.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class EventsController : Controller
{
    private readonly IEventService _service;
    public EventsController(IEventService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        var vm = await _service.AdminGetAllAsync();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var vm = await _service.AdminGetEditAsync(id);
        if (vm == null) return NotFound();

        ViewBag.EventId = id;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdminEventEditVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventId = id;
            return View(vm);
        }

        var ok = await _service.AdminUpdateAsync(id, vm);
        if (!ok)
        {
            ModelState.AddModelError("", "Update failed.");
            ViewBag.EventId = id;
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.AdminDeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}