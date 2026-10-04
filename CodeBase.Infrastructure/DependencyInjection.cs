using CodeBase.Application.Abstractions.Authentication;
using CodeBase.Application.Abstractions.Communication;
using CodeBase.Application.Abstractions.Media;
using CodeBase.Application.Abstractions.Persistence;
using CodeBase.Infrastructure.Authentication;
using CodeBase.Infrastructure.Communication;
using CodeBase.Infrastructure.Media;
using CodeBase.Infrastructure.Persistence;
using CodeBase.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeBase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Features:Database"))
        {
            AddPersistence(services, configuration);
        }

        if (configuration.GetValue<bool>("Features:Jwt"))
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton<ITokenService, JwtTokenService>();
        }

        if (configuration.GetValue<bool>("Features:Mail"))
        {
            services.AddOptions<MailOptions>()
                .Bind(configuration.GetSection(MailOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }

        if (configuration.GetValue<bool>("Features:Cloudinary"))
        {
            services.AddOptions<CloudinaryOptions>()
                .Bind(configuration.GetSection(CloudinaryOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddScoped<IFileStorage, CloudinaryFileStorage>();
        }

        return services;
    }

    private static void AddPersistence(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required when Features:Database is enabled.");
        }

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<AppDbContext>());
    }
}
