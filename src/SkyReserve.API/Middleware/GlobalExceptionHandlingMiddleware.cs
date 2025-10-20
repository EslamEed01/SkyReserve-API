using FluentValidation;
using SkyReserve.Application.Consts;
using System.Text.Json;

namespace SkyReserve.API.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            catch (Exception exception)
            {
                _logger.LogError(exception, "An unhandled exception occurred");
                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, error) = GetErrorResponse(exception);

            var result = Result.Failure(error);
            var response = result.ToProblem();

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(response.Value, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static (int StatusCode, Error Error) GetErrorResponse(Exception exception)
        {
            return exception switch
            {
                ValidationException validationException => (
                    StatusCodes.Status400BadRequest,
                    new Error(
                        "Validation.Failed",
                        string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage)),
                        StatusCodes.Status400BadRequest)
                ),
                ArgumentNullException => (
                    StatusCodes.Status400BadRequest,
                    new Error("BadRequest.ArgumentNull", "A required argument was null", StatusCodes.Status400BadRequest)
                ),
                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    new Error("BadRequest.InvalidArgument", "An invalid argument was provided", StatusCodes.Status400BadRequest)
                ),
                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    new Error("Unauthorized.Access", "Access denied", StatusCodes.Status401Unauthorized)
                ),
                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    new Error("NotFound.Resource", "The requested resource was not found", StatusCodes.Status404NotFound)
                ),
                InvalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    new Error("BadRequest.InvalidOperation", "The requested operation is not valid", StatusCodes.Status400BadRequest)
                ),
                TimeoutException => (
                    StatusCodes.Status408RequestTimeout,
                    new Error("RequestTimeout", "The request timed out", StatusCodes.Status408RequestTimeout)
                ),
                NotImplementedException => (
                    StatusCodes.Status501NotImplemented,
                    new Error("NotImplemented", "This feature is not implemented", StatusCodes.Status501NotImplemented)
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    new Error("InternalServerError", "An internal server error occurred", StatusCodes.Status500InternalServerError)
                )
            };
        }
    }
}