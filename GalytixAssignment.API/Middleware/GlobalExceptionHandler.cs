using GalytixAssignment.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GalytixAssignment.API.Middleware
{
    internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException)
            {
                return false;
            }

            var (statusCode, title, detail) = exception switch
            {
                ApiException apiException => (apiException.StatusCode, apiException.Title, apiException.Message),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred.")
            };

            logger.LogError(exception, "Unhandled exception while processing request {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
