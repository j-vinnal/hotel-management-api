using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp.Filters;

namespace WebApp;

/// <summary>
///     Configures Swagger options.
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary>
    ///     Constructor for ConfigureSwaggerOptions.
    /// </summary>
    /// <param name="provider">API version description provider.</param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) =>
        _provider = provider;
    
    /// <summary>
    ///     Configures SwaggerGen options.
    /// </summary>
    /// <param name="options">SwaggerGen options.</param>
    public void Configure(SwaggerGenOptions options)
    {
        // add all possible api versions found
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo()
                {
                    Title = $"API {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                    Contact = new OpenApiContact
                    {
                    Name = "Jüri Vinnal",
                    Email = "jyri.vinnal@gmail.com",
                    Url = new Uri("https://www.linkedin.com/in/j%C3%BCri-vinnal-7a371a14a/")
                }
                });
        }

        // include xml comments (enable creation in csproj file)
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);


        // use FullName for schemaId - avoids conflicts between classes using the same name (which are in different namespaces)
        options.CustomSchemaIds(i => i.FullName);

        // add security definition for Bearer token
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Description =
                "JWT Authorization header using the Bearer scheme.\r\n<br>" +
                "Enter 'Bearer'[space] and then your token in the text box below.\r\n<br>" +
                "Example: <b>Bearer eyJhbGciOiJIUzUxMiIsIn...</b>\r\n<br>" +
                "You will get the bearer from the <i>account/login</i> or <i>account/register</i> endpoint.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

   
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });

        // Register the X-Road-Client header operation filter
        options.OperationFilter<SwaggerXRoadClientHeaderOperationFilter>();
    }
}
