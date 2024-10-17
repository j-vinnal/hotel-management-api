using System.Security.Claims;
using System.Text.Json;
using App.Domain;
using App.Domain.Identity;
using Base.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;


namespace App.DAL.EF.Seeding;

public static class AppDataInit
{
    private static string SeedDataPath = string.Empty;

    public static void InitializeSeedDataPath(IWebHostEnvironment env, ILogger logger)
    {
        // Set the seed data path relative to the known project structure
        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        var isTesting = Environment.GetEnvironmentVariable("DOTNET_RUNNING_TESTS") == "true";

        if (isDocker && isTesting)
        {
            SeedDataPath = Path.Combine(env.ContentRootPath, "..", "App.DAL.EF", "Seeding", "SeedData");
        }
        else if (isDocker)
        {
            SeedDataPath = Path.Combine(env.ContentRootPath, "App.DAL.EF", "Seeding", "SeedData");
        }
        else
        {
            SeedDataPath = Path.Combine(env.ContentRootPath, "..", "App.DAL.EF", "Seeding", "SeedData");
        }

        logger.LogCritical("Is Docker: {isDocker}, Is Testing: {isTesting}", isDocker, isTesting);
    }

    public static void MigrateDatabase(AppDbContext context)
    {
        context.Database.Migrate();
    }

    public static void DropDatabase(AppDbContext context)
    {
        context.Database.EnsureDeleted();

    }

    public static async Task SeedIdentity(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger logger)
    {
        var adminData = await LoadJsonData<AdminUserData>(Path.Combine(SeedDataPath, "admin.json"));
        var guestData = await LoadJsonData<AdminUserData>(Path.Combine(SeedDataPath, "guest.json"));

        // Create roles
        foreach (var role in RoleConstants.DefaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var appRole = new AppRole { Name = role };
                var result = await roleManager.CreateAsync(appRole);

                if (!result.Succeeded) throw new Exception($"Failed to create role {role}.");
            }
        }

        // Create admin user
        var admin = await userManager.FindByIdAsync(adminData.Id);
        if (admin == null)
        {
            admin = new AppUser
            {
                Id = Guid.Parse(adminData.Id),
                UserName = adminData.UserName,
                Email = adminData.UserName,
                FirstName = adminData.FirstName,
                LastName = adminData.LastName,
                PersonalCode = adminData.PersonalCode
            };
            var result = await userManager.CreateAsync(admin, adminData.Password);

            if (!result.Succeeded) throw new Exception($"Failed to create user {admin.UserName}.");

            var res = await userManager.AddClaimsAsync(admin, new List<Claim>
            {
                new(ClaimTypes.GivenName, admin.FirstName),
                new(ClaimTypes.Surname, admin.LastName),
                new("PersonalCode", admin.PersonalCode)
            });

            await userManager.AddToRoleAsync(admin, RoleConstants.Admin);
        }

        // Create guest user
        var guest = await userManager.FindByIdAsync(guestData.Id);
        if (guest == null)
        {
            guest = new AppUser
            {
                Id = Guid.Parse(guestData.Id),
                UserName = guestData.UserName,
                Email = guestData.UserName,
                FirstName = guestData.FirstName,
                LastName = guestData.LastName,
                PersonalCode = guestData.PersonalCode
            };
            var result = await userManager.CreateAsync(guest, guestData.Password);

            if (!result.Succeeded)
            {
                logger.LogError("Failed to create user {UserName}. Errors: {Errors}", guest.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new Exception($"Failed to create user {guest.UserName}.");
            }

            var res = await userManager.AddClaimsAsync(guest, new List<Claim>
            {
                new(ClaimTypes.GivenName, guest.FirstName),
                new(ClaimTypes.Surname, guest.LastName),
                new("PersonalCode", guest.PersonalCode)
            });

            await userManager.AddToRoleAsync(guest, RoleConstants.Guest);
            logger.LogInformation("Guest user {UserName} created successfully.", guest.UserName);
        }
        else
        {
            logger.LogInformation("Guest user {UserName} already exists.", guest.UserName);
        }
    }

    public static async Task<int> SeedAppData(AppDbContext context)
    {
        var hotels = await LoadJsonData<List<Hotel>>(Path.Combine(SeedDataPath, "hotels.json"));
        var rooms = await LoadJsonData<List<Room>>(Path.Combine(SeedDataPath, "rooms.json"));
        var bookings = await LoadJsonData<List<Booking>>(Path.Combine(SeedDataPath, "bookings.json"));

        var result = 0;

        if (!context.Hotels.Any())
        {
            context.Hotels.AddRange(hotels);
            result += await context.SaveChangesAsync();
        }

        if (!context.Rooms.Any())
        {
            context.Rooms.AddRange(rooms);
            result += await context.SaveChangesAsync();
        }

        if (!context.Bookings.Any())
        {
            context.Bookings.AddRange(bookings);
            result += await context.SaveChangesAsync();
        }

        return result;
    }

    private static async Task<T> LoadJsonData<T>(string filePath)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var data = await JsonSerializer.DeserializeAsync<T>(stream);
        if (data == null) throw new Exception($"Failed to load JSON data from {filePath}");
        return data;
    }
}
