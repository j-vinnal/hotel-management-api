using System.Globalization;
using App.BLL;
using App.Contracts.BLL;
using App.Contracts.DAL;
using App.DAL.EF;
using App.DAL.EF.Seeding;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AutoMapperProfile = App.DAL.EF.AutoMapperProfile;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();
builder.Services.AddScoped<IAppBLL, AppBLL>();


// reference any class from class library to be scanned for mapper configurations
builder.Services.AddAutoMapper(
    typeof(App.BLL.AutoMapperProfile),
    typeof(App.Public.AutoMapperProfile),
    typeof(AutoMapperProfile)
);

builder.Services
    .AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddDefaultUI()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllersWithViews();


var supportedCultures = builder.Configuration
    .GetSection("SupportedCultures")
    .GetChildren()
    .Select(x => new CultureInfo(x.Value))
    .ToArray();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // datetime and currency support
    options.SupportedCultures = supportedCultures;
    // UI translated strings
    options.SupportedUICultures = supportedCultures;
    // if nothing is found, use this
    // TODO: why does it fall back to et-EE, even if default is something else
    options.DefaultRequestCulture =
        new RequestCulture(
            builder.Configuration["DefaultCulture"],
            builder.Configuration["DefaultCulture"]);
    options.SetDefaultCulture(builder.Configuration["DefaultCulture"]);

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        // Order is important, its in which order they will be evaluated
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    };
});


// ===================================================
var app = builder.Build();
// ===================================================


// Set up all the database stuff and seed initial data
SetupAppData(app, app.Configuration);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization(options:
    app.Services.GetService<IOptions<RequestLocalizationOptions>>()?.Value!
);

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.MapRazorPages();

app.Run();


static async void SetupAppData(IApplicationBuilder app, IConfiguration configuration)
{
    //DI engine
    using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
        .CreateScope();
    
    var env = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    AppDataInit.InitializeSeedDataPath(env);

    await using var context = serviceScope.ServiceProvider.GetService<AppDbContext>();


    if (context == null) throw new ApplicationException("Problem in services. Can't initialize DB Context");

    using var userManager = serviceScope.ServiceProvider.GetService<UserManager<AppUser>>();
    using var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<AppRole>>();

    if (userManager == null || roleManager == null)
        throw new ApplicationException("Problem in services. Can't initialize UserManager or RoleManager");

    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<IApplicationBuilder>>();


    if (logger == null) throw new ApplicationException("Problem in services. Can't initialize logger");

    if (context.Database.ProviderName!.Contains("InMemory")) return;


    //TODO: wait for db connection


    if (configuration.GetValue<bool>("DataInit:DropDatabase"))
    {
        logger.LogWarning("Dropping Database");
        AppDataInit.DropDatabase(context);
    }

    if (configuration.GetValue<bool>("DataInit:MigrateDatabase"))
    {
        logger.LogWarning("Migrating Database");
        AppDataInit.MigrateDatabase(context);
    }


    if (configuration.GetValue<bool>("DataInit:SeedIdentity"))
    {
        logger.LogWarning("Seeding identity");
        await AppDataInit.SeedIdentity(userManager, roleManager, logger);
    }

    if (configuration.GetValue<bool>("DataInit:SeedData"))
    {
        logger.LogWarning("Seeding app data...");
        var changedEntries = await AppDataInit.SeedAppData(context);
        logger.LogWarning($"Seeded app data: {changedEntries} entries changed");
    }
}
