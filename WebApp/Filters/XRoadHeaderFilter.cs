using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.Filters;

public class XRoadHeaderFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Check for X-Road-Client header
        var xRoadClient = context.HttpContext.Request.Headers["X-Road-Client"].ToString();
        if (string.IsNullOrEmpty(xRoadClient))
        {
            context.Result = new BadRequestObjectResult("X-Road-Client header is missing.");
        }
    }

    public async void OnActionExecuted(ActionExecutedContext context)
    {
        // Retrieve the X-Road-Service value from the attribute
        var serviceAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<XRoadServiceAttribute>()
            .FirstOrDefault();

        if (serviceAttribute != null)
        {
            context.HttpContext.Response.Headers.Append("X-Road-Service", serviceAttribute.Service);
        }

        context.HttpContext.Response.Headers.Append("X-Road-Id", Guid.NewGuid().ToString());
        var requestHash = await ComputeRequestHashAsync(context.HttpContext.Request);
        context.HttpContext.Response.Headers.Append("X-Road-Request-Hash", requestHash);
    }

    private static async Task<string> ComputeRequestHashAsync(HttpRequest request)
    {
        using var sha512 = SHA512.Create();
        var headers = string.Join("", request.Headers);
        var body = request.Body;

        string bodyContent;
        request.EnableBuffering();
        using (var reader = new StreamReader(body, Encoding.UTF8, true, 1024, true))
        {
            bodyContent = await reader.ReadToEndAsync();
        }
        request.Body.Position = 0;

        var combinedContent = headers + bodyContent;
        var hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(combinedContent));
        return Convert.ToBase64String(hashBytes);
    }
}
