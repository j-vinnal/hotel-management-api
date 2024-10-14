using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApp.Middleware
{
    public class XRoadErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public XRoadErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var errorResponse = new
            {
                type = "Server.ServerProxy.InternalError",
                message = exception.Message,
                detail = Guid.NewGuid().ToString() // Unique error ID for tracking
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.Headers.Append("X-Road-Error", "Server.ServerProxy.InternalError");

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
