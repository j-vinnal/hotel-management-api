using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp.Filters;

/// <summary>
/// Operation filter to add the X-Road-Client header to all API endpoints in Swagger.
/// This is necessary for testing endpoints that require the X-Road-Client header.
/// </summary>
public class SwaggerXRoadClientHeaderOperationFilter : IOperationFilter
{
    /// <summary>
    /// Applies the X-Road-Client header to all operations in the Swagger documentation.
    /// </summary>
    /// <param name="operation">The operation to which the header is added.</param>
    /// <param name="context">The context of the operation filter.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
        {
            operation.Parameters = new List<OpenApiParameter>();
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Road-Client",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            },
            Description = "X-Road-Client header required by X-Road protocol"
        });
    }
}