namespace WebApp.Middleware;

/// <summary>
/// Middleware to ensure the presence of the X-Road-Client header for MVC requests.
/// </summary>
public class XRoadClientHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public XRoadClientHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the request is for an MVC controller
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            // Add the X-Road-Client header if it's missing
            if (!context.Request.Headers.ContainsKey("X-Road-Client"))
            {
                context.Request.Headers.Append("X-Road-Client", "YourDefaultClientValue");
            }
        }

        await _next(context);
    }
}