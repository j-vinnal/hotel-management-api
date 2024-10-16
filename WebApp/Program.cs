using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using App.BLL;
using App.Contracts.BLL;
using App.Contracts.DAL;
using App.DAL.EF;
using App.DAL.EF.Seeding;
using App.Domain.Identity;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp;
using AutoMapperProfile = App.DAL.EF.AutoMapperProfile;
using System.Net.Http.Headers;
using System.Net.Http;
using App.DTO.Public.v1;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebApp.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddControllers();

// Add services to the container.
var useInMemory = builder.Configuration.GetValue<bool>("Database:UseInMemory");

if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

//NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

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


// clear default claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services
    .AddAuthentication()
    .AddCookie(options => { options.SlidingExpiration = true; })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration.GetValue<string>("JWT:issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("JWT:audience"),
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JWT:Key") ??
                                           throw new InvalidOperationException())),
            ClockSkew = TimeSpan.Zero,
        };
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsAllowAll", policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});


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


var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});
apiVersioningBuilder.AddApiExplorer(options =>
{
    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
    // note: the specified format code will format the version as "'v'major[.minor][-status]"
    options.GroupNameFormat = "'v'VVV";

    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
    // can also be used to control the format of the API version in route templates
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

//This configuration ensures that when a model validation error occurs, 
//the API returns a structured error response instead of the default ValidationProblemDetails
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var errorResponse = new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error = string.Join("; ", errors)
            };

            return new BadRequestObjectResult(errorResponse);
        };
    });



// ===================================================
var app = builder.Build();
// ===================================================




// Setup application data
SetupAppData(app, builder.Configuration);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();




//Gives CORS error
//app.UseMiddleware<XRoadHeaderMiddleware>();

app.UseAuthorization();

app.UseCors("CorsAllowAll");


app.UseMiddleware<DateTimeMiddleware>();
// Register the X-Road error handling middleware and 
app.UseMiddleware<XRoadErrorHandlingMiddleware>();

app.UseRequestLocalization(options:
    app.Services.GetService<IOptions<RequestLocalizationOptions>>()?.Value!
);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant()
        );
    }
    // serve from root
    // options.RoutePrefix = string.Empty;
});


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

   // if (context.Database.ProviderName!.Contains("InMemory")) return;


    //TODO: wait for db connection


    if (configuration.GetValue<bool>("DataInit:DropDatabase") && !context.Database.ProviderName!.Contains("InMemory"))
    {
        logger.LogWarning("Dropping Database");
        AppDataInit.DropDatabase(context);
        
    }

    if (configuration.GetValue<bool>("DataInit:MigrateDatabase") && !context.Database.ProviderName!.Contains("InMemory"))
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

// needed for unit testing, to change generated top level statement class to public
public partial class Program
{
}



