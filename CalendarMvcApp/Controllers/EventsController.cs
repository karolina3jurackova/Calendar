using Calendar.Application.Abstraction.Services;
using Calendar.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Controllers;

public class EventsController : Controller
{
    private readonly IEventService _service;

    public EventsController(IEventService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        Guid userId = Guid.Empty; // dočasne, kým nebude Identity
        var vm = await _service.GetMyEventsAsync(userId);
        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        Guid userId = Guid.Empty;
        var vm = await _service.GetDetailAsync(id, userId);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(EventCreateVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        Guid userId = Guid.Empty;
        Guid id = await _service.CreateAsync(userId, vm);
        return RedirectToAction(nameof(Details), new { id });
    }
}