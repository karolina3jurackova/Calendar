using Calendar.Infrastructure.Identity;
using CalendarMvcApp.Models.Admin.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CalendarMvcApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: /Admin/Users
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();

        var vm = new List<AdminUserListItemVM>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            vm.Add(new AdminUserListItemVM
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "",
                Roles = roles
            });
        }

        return View(vm);
    }

    // GET: /Admin/Users/Edit/{id}
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var vm = new AdminUserEditVM
        {
            Id = user.Id,
            Email = user.Email ?? user.UserName ?? "",
            IsAdmin = roles.Contains("Admin"),
            IsManager = roles.Contains("Manager"),
            IsCustomer = roles.Contains("Customer")
        };

        return View(vm);
    }

    // POST: /Admin/Users/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdminUserEditVM vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        // ✅ admin môže meniť email/username, ALE nie password hash
        user.Email = vm.Email;
        user.UserName = vm.Email;

        var updateRes = await _userManager.UpdateAsync(user);
        if (!updateRes.Succeeded)
        {
            foreach (var e in updateRes.Errors)
                ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        // role sync
        var desired = new List<string>();
        if (vm.IsAdmin) desired.Add("Admin");
        if (vm.IsManager) desired.Add("Manager");
        if (vm.IsCustomer) desired.Add("Customer");

        // aspoň Customer nech ostane
        if (desired.Count == 0) desired.Add("Customer");

        var currentRoles = await _userManager.GetRolesAsync(user);

        var toAdd = desired.Except(currentRoles).ToArray();
        var toRemove = currentRoles.Except(desired).ToArray();

        if (toRemove.Length > 0)
            await _userManager.RemoveFromRolesAsync(user, toRemove);

        if (toAdd.Length > 0)
            await _userManager.AddToRolesAsync(user, toAdd);

        return RedirectToAction(nameof(Index));
    }
}