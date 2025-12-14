using Calendar.Application.Abstraction.Repositories;
using Calendar.Application.Abstraction.Services;
using Calendar.Application.Implementation;
using Calendar.Infrastructure;
using Calendar.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(); // MVC views
builder.Services.AddRazorPages();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Events}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();