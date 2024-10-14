using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace WebApp.Middleware;

public class XRoadHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public XRoadHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Bypass middleware logic for OPTIONS requests
        if (context.Request.Method == HttpMethods.Options)
        {
            await _next(context);
            return;
        }

        // Add X-Road headers to the response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append("X-Road-Client", "EE/COM/12345678/SYSTEM1");
            context.Response.Headers.Append("X-Road-Service", "EE/COM/12345678/SYSTEM1/exampleService/v1");
            context.Response.Headers.Append("X-Road-Id", Guid.NewGuid().ToString());
            context.Response.Headers.Append("X-Road-UserId", "EE12345678901");
            context.Response.Headers.Append("X-Road-ProtocolVersion", "4.0");

            // Add X-Road-Request-Hash for responses
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                context.Response.Headers.Append("X-Road-Request-Hash", ComputeRequestHash(context.Request));
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private string ComputeRequestHash(HttpRequest request)
    {
        using (var sha512 = SHA512.Create())
        {
            // Combine headers and body for hashing
            var headers = string.Join("", request.Headers);
            var body = request.Body;

            // Read the request body
            string bodyContent;
            request.EnableBuffering(); 
            using (var reader = new System.IO.StreamReader(body, Encoding.UTF8, true, 1024, true))
            {
                bodyContent = reader.ReadToEnd();
            }
            request.Body.Position = 0; 

            // Compute hash
            var combinedContent = headers + bodyContent;
            var hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(combinedContent));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
