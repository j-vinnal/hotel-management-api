using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApp.Exceptions;

namespace WebApp.Filters;

public class XRoadExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        string errorType;
        int statusCode;

        switch (context.Exception)
        {
            case BadRequestException:
                errorType = "Client.BadRequest";
                statusCode = (int)HttpStatusCode.BadRequest;
                break;
            case NotFoundException:
                errorType = "Client.NotFound";
                statusCode = (int)HttpStatusCode.NotFound;
                break;
            case Microsoft.EntityFrameworkCore.DbUpdateException:
                errorType = "Server.ServerProxy.DatabaseError";
                statusCode = (int)HttpStatusCode.InternalServerError;
                break;
            case HttpRequestException:
                errorType = "Server.ServerProxy.NetworkError";
                statusCode = (int)HttpStatusCode.ServiceUnavailable;
                break;
            default:
                errorType = "Server.ServerProxy.InternalError";
                statusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var errorResponse = new
        {
            type = errorType,
            message = context.Exception.Message,
            detail = Guid.NewGuid().ToString(),
        };

        context.HttpContext.Response.ContentType = "application/json";
        context.HttpContext.Response.StatusCode = statusCode;
        context.HttpContext.Response.Headers.Append("X-Road-Error", errorType);

        context.Result = new JsonResult(errorResponse);
    }
}