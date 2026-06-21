using Microsoft.AspNetCore.Mvc;
using ServiceImplementation.Exceptions;
using System.Net;
using System.Text.Json;

namespace Horr.Middleware
{
    /// <summary>
    /// Global exception handler that converts domain exceptions into
    /// a format matching the frontend's expectations.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (ex is not ValidationException && ex is not UnauthorizedAccessException && ex is not ForbiddenException && ex is not NotFoundException && ex is not ConflictException && ex is not InvalidStateException)
                {
                    _logger.LogError(ex, "Unhandled exception");
                }
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;
            object errorResponse;

            switch (exception)
            {
                case ValidationException valEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    // Standard ASP.NET Core Validation problem details format
                    errorResponse = new
                    {
                        title = "One or more validation errors occurred.",
                        status = response.StatusCode,
                        errors = valEx.Errors
                    };
                    break;

                case NotFoundException nfEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new { message = nfEx.Message, errorCode = "NOT_FOUND" };
                    break;

                case ConflictException cfEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse = new { message = cfEx.Message, errorCode = "CONFLICT" };
                    break;

                case InvalidStateException isEx:
                    response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    errorResponse = new { message = isEx.Message, errorCode = "INVALID_STATE" };
                    break;

                case ForbiddenException fbEx:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse = new { message = fbEx.Message, errorCode = "FORBIDDEN" };
                    break;

                case UnauthorizedAccessException uaEx:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse = new { message = uaEx.Message, errorCode = "UNAUTHORIZED" };
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new { message = "An internal server error occurred.", errorCode = "SERVER_ERROR", detail = exception.ToString() };
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await response.WriteAsync(result);
        }
    }
}
