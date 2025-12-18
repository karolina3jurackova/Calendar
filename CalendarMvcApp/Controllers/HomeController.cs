using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CalendarMvcApp.Models;

namespace CalendarMvcApp.Controllers;

// HOME - landing stránka + error stránka
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger; // pripravené pre logovanie

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Home page (napr. UI kalendár)
    public IActionResult Index()
    {
        return View();
    }

    // ERROR page (zachytáva výnimky)
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}