using Calendar.Infrastructure.Data;
using Calendar.Infrastructure.Identity;
using Calendar.Infrastructure.Repositories;
using Calendar.Application.Implementation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CalendarMvcApp.Controllers;

public class EventsControllerTests
{
    [Fact]
    public async Task Index_Returns_View_With_Events_From_Database()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var store = new UserStore<ApplicationUser, ApplicationRole, AppDbContext, Guid>(db);
        var userManager = new UserManager<ApplicationUser>(
            store,
            null!,
            new PasswordHasher<ApplicationUser>(),
            new IUserValidator<ApplicationUser>[0],
            new IPasswordValidator<ApplicationUser>[0],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            null!
        );

        var user = new ApplicationUser { UserName = "t@test.com", Email = "t@test.com" };
        await userManager.CreateAsync(user, "Pass123$");

        db.Events.Add(new Calendar.Domain.Entities.Event
        {
            Id = Guid.NewGuid(),
            OwnerId = user.Id,
            Title = "Test event",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            DateCreated = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var repo = new EventRepository(db);
        var service = new EventService(repo);

        var controller = new EventsController(service, userManager);

        controller.ControllerContext = TestHelpers.CreateControllerContextWithUserId(user.Id);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IList<Calendar.Application.ViewModels.EventListItemVM>>(view.Model);

        Assert.Single(model);
        Assert.Equal("Test event", model[0].Title);
    }
}