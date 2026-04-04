using Microsoft.Extensions.DependencyInjection;

namespace Aspu.Common.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services) =>
        services;
}
