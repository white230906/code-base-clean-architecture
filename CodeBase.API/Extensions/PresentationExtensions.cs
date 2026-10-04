using CodeBase.API.Contracts;
using CodeBase.API.Services;
using CodeBase.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace CodeBase.API.Extensions;

public static class PresentationExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .ToDictionary(
                            entry => entry.Key,
                            entry => entry.Value!.Errors
                                .Select(error => error.ErrorMessage)
                                .ToArray());

                    var response = new ApiErrorResponse(
                        "Validation failed",
                        StatusCodes.Status400BadRequest,
                        "One or more validation errors occurred.",
                        "VALIDATION_FAILED",
                        errors,
                        context.HttpContext.TraceIdentifier,
                        DateTimeOffset.UtcNow);

                    return new BadRequestObjectResult(response);
                };
            });
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHealthChecks();
        services.AddAuthenticationServices(configuration);
        services.AddCorsPolicy(configuration);
        services.AddApiRateLimiting(configuration);
        services.AddSwaggerServices();

        return services;
    }
}
