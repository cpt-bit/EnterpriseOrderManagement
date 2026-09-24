using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderProcessing.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.TraceIdentifier;
        
        logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            httpContext.Request.Path,
            httpContext.Request.Method);

        var (statusCode, title, detail) = exception switch
        {
            ArgumentNullException or ArgumentException =>
                (StatusCodes.Status400BadRequest, "Invalid Request", "One or more request parameters are invalid"),
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Not Found", "The requested resource could not be found"),
            FileNotFoundException =>
                (StatusCodes.Status404NotFound, "Not Found", "The requested file could not be found"),
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication credentials are required"),
            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Conflict", "The operation cannot be performed in the current state"),
            TimeoutException =>
                (StatusCodes.Status504GatewayTimeout, "Timeout", "The request processing exceeded the timeout limit"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Type = $"https://api.example.com/errors/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}"
        };
        
        problemDetails.Extensions["traceId"] = correlationId;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}