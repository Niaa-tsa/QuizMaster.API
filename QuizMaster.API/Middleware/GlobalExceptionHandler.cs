using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuizMaster.Service.Exceptions;

namespace QuizMaster.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = MapException(exception);

        if (status >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception: {Message}",
                exception.Message);
        }
        else
        {
            _logger.LogWarning(
                "Request failed with {Status}: {Message}",
                status,
                exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    private static (int Status, string Title, string Detail) MapException(
        Exception exception)
    {
        return exception switch
        {
            UnauthorizedException e =>
    (StatusCodes.Status401Unauthorized, "Unauthorized", e.Message),
            NotFoundException e =>
                (StatusCodes.Status404NotFound, "Resource not found", e.Message),

            ForbiddenException e =>
                (StatusCodes.Status403Forbidden, "Forbidden", e.Message),

            ConflictException e =>
                (StatusCodes.Status409Conflict, "Conflict", e.Message),

            BusinessRuleException e =>
                (StatusCodes.Status400BadRequest, "Business rule violation", e.Message),

            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } =>
                (StatusCodes.Status409Conflict, "Conflict",
                 "A record with the same unique value already exists."),

            _ =>
                (StatusCodes.Status500InternalServerError, "Server error",
                 "An unexpected error occurred.")
        };
    }
}