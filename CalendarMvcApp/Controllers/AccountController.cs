using Calendar.Infrastructure.Identity;
using CalendarMvcApp.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;     // práca s používateľmi (create, role, find...)
    private readonly SignInManager<ApplicationUser> _signInManager; // prihlasovanie/odhlasovanie (cookies)

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // zobrazí formulár)
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View(new RegisterVM());

    // vytvorí usera, dá rolu, prihlási)
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM vm)
    {
        if (!ModelState.IsValid) return View(vm); // serverová validácia

        var user = new ApplicationUser
        {
            UserName = vm.Email, //tu dávame email
            Email = vm.Email
        };

        var result = await _userManager.CreateAsync(user, vm.Password); // vytvorenie účtu + hash hesla
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, "Customer"); // default rola po registrácii

        await _signInManager.SignInAsync(user, isPersistent: false); // prihlásenie hneď po registrácii
        return RedirectToAction("Index", "Events"); // po prihlásení ide na kalendár
    }

    // LOGIN - GET 
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl; // kam sa má user vrátiť po login
        return View(new LoginVM());
    }

    // LOGIN - POST (overí heslo, prihlási cookie)
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _signInManager.PasswordSignInAsync(
            vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Nesprávny email alebo heslo.");
            return View(vm);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Events");
    }

    // LOGOUT 
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // ACCESS DENIED (keď nemá rolu/práva)
    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();
}