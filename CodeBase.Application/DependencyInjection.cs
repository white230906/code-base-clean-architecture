using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SampleService = CodeBase.Application.Services.Sample;

namespace CodeBase.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<SampleService.IService, SampleService.Service>();

        return services;
    }
}
