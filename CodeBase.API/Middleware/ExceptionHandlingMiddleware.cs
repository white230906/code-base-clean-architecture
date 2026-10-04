using CodeBase.API.Contracts;
using CodeBase.Application.Common.Exceptions;
using FluentValidation;

namespace CodeBase.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request disconnected while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var error = MapException(exception, context.TraceIdentifier);

            if (error.Status >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    "Request {Method} {Path} failed with {MessageCode}",
                    context.Request.Method,
                    context.Request.Path,
                    error.MessageCode);
            }

            context.Response.Clear();
            context.Response.StatusCode = error.Status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(error, context.RequestAborted);
        }
    }

    private static ApiErrorResponse MapException(
        Exception exception,
        string traceId)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            return CreateError(
                StatusCodes.Status400BadRequest,
                "Validation failed",
                "VALIDATION_FAILED",
                "One or more validation errors occurred.",
                errors,
                traceId);
        }

        if (exception is AppException appException)
        {
            var (status, title) = appException.ErrorType switch
            {
                ErrorType.Validation => (400, "Validation failed"),
                ErrorType.Unauthorized => (401, "Unauthorized"),
                ErrorType.Forbidden => (403, "Forbidden"),
                ErrorType.NotFound => (404, "Not found"),
                ErrorType.Conflict => (409, "Conflict"),
                _ => (500, "Internal server error")
            };

            return CreateError(
                status,
                title,
                appException.MessageCode,
                status >= 500 ? "An unexpected error occurred." : appException.Message,
                appException.Errors,
                traceId);
        }

        return CreateError(
            StatusCodes.Status500InternalServerError,
            "Internal server error",
            "INTERNAL_SERVER_ERROR",
            "An unexpected error occurred.",
            null,
            traceId);
    }

    private static ApiErrorResponse CreateError(
        int status,
        string title,
        string messageCode,
        string detail,
        object? errors,
        string traceId) => new(
            title,
            status,
            detail,
            messageCode,
            errors,
            traceId,
            DateTimeOffset.UtcNow);
}
