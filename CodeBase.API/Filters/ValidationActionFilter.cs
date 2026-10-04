using CodeBase.API.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodeBase.API.Filters;

public sealed class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (context.ModelState.IsValid)
        {
            await next();
            return;
        }

        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The supplied value is invalid."
                        : error.ErrorMessage)
                    .ToArray());

        var response = new ApiErrorResponse(
            "Validation failed",
            StatusCodes.Status400BadRequest,
            "One or more validation errors occurred.",
            "VALIDATION_FAILED",
            errors,
            context.HttpContext.TraceIdentifier,
            DateTimeOffset.UtcNow);

        context.Result = new BadRequestObjectResult(response);
    }
}
