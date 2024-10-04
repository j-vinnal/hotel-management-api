using System.Security.Claims;
using System.Text.Json;
using App.Domain;
using App.Domain.Identity;
using Base.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace App.DAL.EF.Seeding;

public static class AppDataInit
{
    private static string SeedDataPath = string.Empty;

    public static void InitializeSeedDataPath(IWebHostEnvironment env)
    {
        // Check if running in Docker
        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        if (isDocker)
        {
            SeedDataPath = Path.Combine(env.ContentRootPath, "App.DAL.EF", "Seeding", "SeedData");
        }
        else
        {
            SeedDataPath = Path.Combine(env.ContentRootPath, "..", "App.DAL.EF", "Seeding", "SeedData");
        }
    }

    public static void MigrateDatabase(AppDbContext context)
    {
        context.Database.Migrate();
    }

    public static void DropDatabase(AppDbContext context)
    {
        context.Database.EnsureDeleted();
    }

    public static async Task SeedIdentity(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
        ILogger logger)
    {
        var adminData = await LoadJsonData<AdminUserData>(Path.Combine(SeedDataPath, "admin.json"));

        // Create admin role
        if (!await roleManager.RoleExistsAsync(RoleConstants.Admin))
        {
            var role = new AppRole { Name = RoleConstants.Admin };
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded) throw new Exception($"Failed to create role {RoleConstants.Admin}.");
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
                PersonalCode =  adminData.PersonalCode
            };
            var result = await userManager.CreateAsync(admin, adminData.Password);
            
            if (!result.Succeeded) throw new Exception($"Failed to create user {admin.UserName}.");

            var res = await userManager.AddClaimsAsync(admin, new List<Claim>
            {
                new(ClaimTypes.GivenName, admin.FirstName),
                new(ClaimTypes.Surname, admin.LastName)
            });

            await userManager.AddToRoleAsync(admin, RoleConstants.Admin);
        }
    }

    public static async Task<int> SeedAppData(AppDbContext context)
    {
        var hotels = await LoadJsonData<List<Hotel>>(Path.Combine(SeedDataPath, "hotels.json"));
        var rooms = await LoadJsonData<List<Room>>(Path.Combine(SeedDataPath, "rooms.json"));

        var result = 0;

        if (!context.Hotels.Any())
        {
            context.Hotels.AddRange(hotels);
            result = +await context.SaveChangesAsync();
        }

        if (!context.Rooms.Any())
        {
            context.Rooms.AddRange(rooms);
            result = +await context.SaveChangesAsync();
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