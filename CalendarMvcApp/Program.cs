using Calendar.Infrastructure;
using Calendar.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// všetko (DbContext + Identity + repo + services) ide cez Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 🔴 KROK 2.2 – HTTPS redirect (CHÝBAL)
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 🔴 Seed rolí + admin účtu (správne miesto)
using (var scope = app.Services.CreateScope())
{
    await IdentitySeed.SeedAsync(scope.ServiceProvider);
}

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Events}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();