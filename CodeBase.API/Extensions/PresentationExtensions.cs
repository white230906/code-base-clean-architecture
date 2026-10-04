using CodeBase.API.Services;
using CodeBase.Application.Abstractions.Authentication;

namespace CodeBase.API.Extensions;

public static class PresentationExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
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
