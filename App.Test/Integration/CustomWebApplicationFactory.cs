using App.DAL.EF;
using App.DAL.EF.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using App.Domain.Identity;

namespace App.Test.Integration;

public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup: class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            // Get the configuration
            var configuration = context.Configuration;
           

            // Find DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<AppDbContext>));

            // If found - remove
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Check if we should use in-memory database
            if (configuration.GetValue<bool>("Testing:UseInMemoryDatabase"))
            {
                // Use in-memory database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            }
            else
            {
                // Use PostgreSQL test database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(configuration.GetConnectionString("TestConnection"));
                });
            }

            // Create db and seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            var logger = scopedServices
                .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

            // Initialize seed data path
            var env = scopedServices.GetRequiredService<IWebHostEnvironment>();
            AppDataInit.InitializeSeedDataPath(env, logger);

            var userManager = scopedServices.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scopedServices.GetRequiredService<RoleManager<AppRole>>();

            // Ensure database is deleted and created
            //db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            try
            {
                logger.LogInformation("Starting to seed identity data...");
                AppDataInit.SeedIdentity(userManager, roleManager, logger).Wait();
                logger.LogInformation("Identity data seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the identity data.");
                throw; // Rethrow the exception if needed
            }

            try
            {
                AppDataInit.SeedAppData(db).Wait();
                logger.LogInformation("Database seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database.");
            }
        });
    }
}
