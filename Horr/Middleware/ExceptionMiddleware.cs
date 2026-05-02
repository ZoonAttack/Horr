using Microsoft.AspNetCore.Mvc;
using ServiceImplementation.Exceptions;
using System.Text.Json;

namespace Horr.Middleware
{
    /// <summary>
    /// Global exception handler that converts domain exceptions into
    /// RFC 7807 ProblemDetails responses, matching the project's error pattern.
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
            catch (NotFoundException ex)
            {
                await WriteProblemDetails(context, StatusCodes.Status404NotFound, "Not Found", ex.Message);
            }
            catch (ValidationException ex)
            {
                // 400 Bad Request — includes field-level errors
                var problem = new ValidationProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Failed",
                    Detail = ex.Message
                };
                foreach (var error in ex.Errors)
                {
                    // Group errors under a generic "fields" key; individual field
                    // names are embedded in the message strings by the handlers.
                    problem.Errors[""] = problem.Errors.TryGetValue("", out var current)
                        ? current.Append(error).ToArray()
                        : new[] { error };
                }
                await WriteResponse(context, StatusCodes.Status400BadRequest, problem);
            }
            catch (ConflictException ex)
            {
                await WriteProblemDetails(context, StatusCodes.Status409Conflict, "Conflict", ex.Message);
            }
            catch (InvalidStateException ex)
            {
                await WriteProblemDetails(context, StatusCodes.Status422UnprocessableEntity, "Invalid State", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteProblemDetails(context, StatusCodes.Status403Forbidden, "Forbidden", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteProblemDetails(context, StatusCodes.Status500InternalServerError, "Server Error",
                    ex.ToString());
            }
        }

        private static async Task WriteProblemDetails(HttpContext context, int status, string title, string detail)
        {
            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            };
            await WriteResponse(context, status, problem);
        }

        private static async Task WriteResponse(HttpContext context, int status, object body)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = status;
            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(json);
        }
    }
}
